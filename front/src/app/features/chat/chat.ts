import { Component, inject, signal, computed, OnInit, ElementRef, ViewChild, AfterViewChecked } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ChatService } from '../../core/services/chat.service';
import { AuthService } from '../../core/services/auth.service';
import { MockDataService } from '../../core/services/mock-data.service';
import { Conversation, Message } from '../../core/models/interfaces';
import { ToastService } from '../../core/services/toast.service';

@Component({
  selector: 'app-chat',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  template: `
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8 bg-slate-50 dark:bg-slate-900 transition-colors duration-300">
      
      <div class="bg-white dark:bg-slate-950 rounded-2xl border border-slate-150 dark:border-slate-850 shadow-lg h-[75vh] flex overflow-hidden">
        
        <!-- RIGHT PANEL: CONVERSATIONS LIST (RTL) -->
        <div class="w-full md:w-80 lg:w-96 border-l border-slate-150 dark:border-slate-850 flex flex-col h-full bg-white dark:bg-slate-950" [class.hidden]="activeConv() && isMobileChatView()">
          
          <!-- Search Header -->
          <div class="p-4 border-b border-slate-100 dark:border-slate-850 space-y-3 text-right">
            <h2 class="text-lg font-black text-slate-800 dark:text-white">المحادثات</h2>
            <div class="relative">
              <input 
                type="text" 
                [(ngModel)]="searchQuery"
                placeholder="ابحث عن صنايعي أو عميل..."
                class="w-full pl-4 pr-10 py-2.5 bg-slate-50 dark:bg-slate-900 border border-slate-200 dark:border-slate-800 text-xs rounded-xl outline-none focus:border-primary text-right"
              >
              <span class="absolute right-3.5 top-1/2 -translate-y-1/2 text-slate-450">🔍</span>
            </div>
          </div>

          <!-- Conversations List -->
          <div class="flex-grow overflow-y-auto divide-y divide-slate-50 dark:divide-slate-900">
            @for (conv of filteredConvs(); track conv.id) {
              <div 
                (click)="selectConversation(conv)"
                [class]="activeConv()?.id === conv.id ? 'bg-primary/5 border-r-4 border-primary' : 'hover:bg-slate-50 dark:hover:bg-slate-900/40'"
                class="p-4 flex items-center gap-3 cursor-pointer transition-colors text-right"
              >
                <!-- Avatar with online status -->
                <div class="relative flex-shrink-0">
                  <img [src]="conv.otherUser.avatar" class="w-11 h-11 rounded-xl object-cover">
                  <span class="absolute bottom-0 right-0 w-3 h-3 bg-accent border-2 border-white dark:border-slate-950 rounded-full"></span>
                </div>

                <div class="flex-grow space-y-1 overflow-hidden">
                  <div class="flex items-center justify-between">
                    <span class="text-xs text-slate-400 font-bold">{{ conv.lastMessage.timestamp }}</span>
                    <h3 class="text-xs sm:text-sm font-black text-slate-800 dark:text-white truncate">{{ conv.otherUser.name }}</h3>
                  </div>
                  
                  <div class="flex items-center justify-between text-xs text-slate-500 dark:text-slate-400">
                    @if (conv.unreadCount > 0) {
                      <span class="px-2 py-0.5 bg-red-500 text-white font-extrabold text-[10px] rounded-full">{{ conv.unreadCount }}</span>
                    } @else {
                      <span></span>
                    }
                    <p class="truncate max-w-[150px] font-semibold leading-normal">{{ conv.lastMessage.content }}</p>
                  </div>
                </div>
              </div>
            } @empty {
              <div class="text-center py-12 text-xs text-slate-450 dark:text-slate-550">لا توجد محادثات نشطة حالياً.</div>
            }
          </div>

        </div>

        <!-- LEFT PANEL: MESSAGES FEED (RTL) -->
        <div class="flex-grow flex flex-col h-full bg-slate-50/50 dark:bg-slate-900/20" [class.hidden]="!activeConv() && isMobileChatView()">
          
          @if (activeConv(); as conv) {
            
            <!-- Feed Header -->
            <div class="px-6 py-4 bg-white dark:bg-slate-950 border-b border-slate-100 dark:border-slate-850 flex items-center justify-between">
              
              <!-- Back button on mobile -->
              <button 
                (click)="activeConv.set(null)"
                class="md:hidden p-2 rounded-lg hover:bg-slate-100 dark:hover:bg-slate-900 text-slate-600 dark:text-slate-350 cursor-pointer"
              >
                &larr; رجوع
              </button>

              <div class="flex items-center gap-3 text-right">
                <div class="relative">
                  <img [src]="conv.otherUser.avatar" class="w-10 h-10 rounded-xl object-cover">
                  <span class="absolute bottom-0 right-0 w-2.5 h-2.5 bg-accent border-2 border-white dark:border-slate-950 rounded-full"></span>
                </div>
                <div class="flex flex-col">
                  <h3 class="text-xs sm:text-sm font-black text-slate-800 dark:text-white">{{ conv.otherUser.name }}</h3>
                  <span class="text-[10px] text-slate-400 font-bold">{{ conv.otherUser.profession || 'عميل' }}</span>
                </div>
              </div>

              <div class="flex items-center gap-2">
                <span class="w-2.5 h-2.5 rounded-full bg-accent animate-ping"></span>
                <span class="text-[10px] font-bold text-accent">متصل الآن</span>
              </div>
            </div>

            <!-- Messages List -->
            <div #scrollContainer class="flex-grow p-6 overflow-y-auto space-y-4">
              @for (msg of activeMessages(); track msg.id) {
                <div 
                  [class]="msg.senderId === currentUser()?.id ? 'justify-end' : 'justify-start'"
                  class="flex w-full"
                >
                  <!-- Bubble -->
                  <div 
                    [class]="msg.senderId === currentUser()?.id 
                      ? 'bg-primary text-white rounded-2xl rounded-tr-none' 
                      : 'bg-white dark:bg-slate-950 border border-slate-100 dark:border-slate-850 text-slate-800 dark:text-slate-200 rounded-2xl rounded-tl-none'"
                    class="p-4 max-w-sm sm:max-w-md shadow-sm relative space-y-2 text-right"
                  >
                    @if (msg.image) {
                      <div class="rounded-xl overflow-hidden max-h-48 mb-2">
                        <img [src]="msg.image" class="w-full h-full object-cover">
                      </div>
                    }
                    <p class="text-xs sm:text-sm leading-relaxed font-semibold">{{ msg.content }}</p>
                    <span 
                      [class]="msg.senderId === currentUser()?.id ? 'text-white/70' : 'text-slate-400'"
                      class="text-[9px] block text-left"
                    >
                      {{ msg.timestamp }}
                    </span>
                  </div>
                </div>
              }

              <!-- Typing Indicator Mock -->
              @if (isTyping()) {
                <div class="flex justify-start w-full animate-pulse">
                  <div class="bg-white dark:bg-slate-950 border border-slate-100 dark:border-slate-850 p-3 rounded-2xl rounded-tl-none flex items-center gap-1.5">
                    <span class="w-2 h-2 rounded-full bg-slate-350 dark:bg-slate-500 animate-bounce" style="animation-delay: 0ms"></span>
                    <span class="w-2 h-2 rounded-full bg-slate-350 dark:bg-slate-500 animate-bounce" style="animation-delay: 150ms"></span>
                    <span class="w-2 h-2 rounded-full bg-slate-350 dark:bg-slate-500 animate-bounce" style="animation-delay: 300ms"></span>
                  </div>
                </div>
              }
            </div>

            <!-- Typing Input Bar -->
            <div class="p-4 bg-white dark:bg-slate-950 border-t border-slate-100 dark:border-slate-850">
              
              <!-- Quick Attachment Toolbar -->
              <div class="flex justify-between items-center pb-2.5 mb-2.5 border-b border-slate-50 dark:border-slate-900/50">
                <div class="flex items-center gap-3">
                  <!-- Image Upload Button -->
                  <button 
                    (click)="simulateImageAttach()" 
                    class="p-2 bg-slate-50 dark:bg-slate-900 hover:bg-primary/10 hover:text-primary rounded-xl text-slate-500 dark:text-slate-400 transition-colors cursor-pointer"
                    title="إرفاق صورة"
                  >
                    📷
                  </button>
                  
                  <!-- Emoji Button -->
                  <button 
                    (click)="addEmoji('👍')" 
                    class="p-2 bg-slate-50 dark:bg-slate-900 hover:bg-primary/10 hover:text-primary rounded-xl text-slate-500 dark:text-slate-400 transition-colors cursor-pointer"
                    title="لايك"
                  >
                    👍
                  </button>
                  <button 
                    (click)="addEmoji('🛠️')" 
                    class="p-2 bg-slate-50 dark:bg-slate-900 hover:bg-primary/10 hover:text-primary rounded-xl text-slate-500 dark:text-slate-400 transition-colors cursor-pointer"
                    title="مفتاح ربط"
                  >
                    🛠️
                  </button>
                </div>

                @if (attachedImage()) {
                  <span class="text-[10px] font-bold text-accent bg-accent/10 px-2.5 py-0.5 rounded-full animate-slide-up flex items-center gap-1.5">
                    <span>📷 تم إرفاق صورة للمعاينة</span>
                    <button (click)="attachedImage.set('')" class="text-red-500 font-extrabold hover:text-red-700">×</button>
                  </span>
                }
              </div>

              <!-- Main Input Area -->
              <form (submit)="onSend()" class="flex items-center gap-3">
                <button 
                  type="submit" 
                  class="px-5 py-3 bg-primary hover:bg-primary-hover text-white font-bold rounded-xl shadow-md hover:shadow-lg transition-all text-xs sm:text-sm flex-shrink-0 cursor-pointer"
                >
                  إرسال
                </button>
                
                <input 
                  type="text" 
                  [(ngModel)]="messageText" 
                  name="message" 
                  placeholder="اكتب رسالتك هنا..." 
                  class="flex-grow px-4 py-3 bg-slate-50 dark:bg-slate-900 border border-slate-200 dark:border-slate-800 text-xs sm:text-sm text-slate-800 dark:text-white rounded-xl outline-none focus:border-primary text-right font-medium"
                >
              </form>
            </div>

          } @else {
            <!-- No Conversation Selected State -->
            <div class="flex-grow flex flex-col items-center justify-center text-center p-12 space-y-5">
              <div class="w-20 h-20 rounded-full bg-slate-100 dark:bg-slate-900 flex items-center justify-center text-4xl shadow-sm">💬</div>
              <h3 class="text-base sm:text-lg font-black text-slate-700 dark:text-slate-250">اختر محادثة للبدء في التواصل</h3>
              <p class="text-xs sm:text-sm text-slate-400 max-w-xs leading-relaxed">
                اضغط على أي مستخدم في القائمة الجانبية لبدء المحادثة ومناقشة تفاصيل الحجز وأعمال الصيانة.
              </p>
            </div>
          }

        </div>

      </div>

    </div>
  `,
  styles: [`
    @keyframes slide-up {
      from { transform: translateY(5px); opacity: 0; }
      to { transform: translateY(0); opacity: 1; }
    }
    .animate-slide-up {
      animation: slide-up 0.2s ease-out forwards;
    }
  `]
})
export default class ChatComponent implements OnInit, AfterViewChecked {
  @ViewChild('scrollContainer') private scrollContainer!: ElementRef;

  chatService = inject(ChatService);
  authService = inject(AuthService);
  private mockData = inject(MockDataService);
  private route = inject(ActivatedRoute);
  private toast = inject(ToastService);

  searchQuery = '';
  messageText = '';
  attachedImage = signal<string>('');

  // Active states
  activeConv = signal<Conversation | null>(null);
  isTyping = signal<boolean>(false);

  currentUser = this.authService.currentUser;

  // Filter conversations
  filteredConvs = computed(() => {
    const list = this.chatService.conversations();
    const query = this.searchQuery.trim().toLowerCase();
    if (!query) return list;

    return list.filter(c => 
      c.otherUser.name.toLowerCase().includes(query) ||
      (c.otherUser.profession && c.otherUser.profession.toLowerCase().includes(query))
    );
  });

  // Active messages computed list
  activeMessages = computed(() => {
    const conv = this.activeConv();
    const me = this.currentUser();
    if (!conv || !me) return [];

    return this.chatService.getMessagesBetween(me.id, conv.otherUser.id);
  });

  ngOnInit() {
    this.route.queryParams.subscribe(params => {
      const withUserId = params['with'];
      if (withUserId) {
        const worker = this.mockData.workers().find(w => w.id === withUserId);
        if (worker) {
          const existing = this.chatService.conversations().find(c => c.otherUser.id === withUserId);
          if (existing) {
            this.activeConv.set(existing);
          } else {
            const tempConv: Conversation = {
              id: 'temp-' + Date.now(),
              otherUser: {
                id: worker.id,
                name: worker.name,
                avatar: worker.avatar,
                role: 'worker',
                profession: worker.profession
              },
              lastMessage: {
                id: 'init',
                senderId: worker.id,
                receiverId: this.currentUser()?.id || '',
                content: 'مرحباً بك! تواصل معي لتفاصيل العمل.',
                timestamp: new Date().toISOString(),
                read: true
              },
              unreadCount: 0
            };
            this.activeConv.set(tempConv);
          }
        }
      }
    });

    this.scrollToBottom();
  }

  ngAfterViewChecked() {
    this.scrollToBottom();
  }

  scrollToBottom(): void {
    try {
      if (this.scrollContainer) {
        this.scrollContainer.nativeElement.scrollTop = this.scrollContainer.nativeElement.scrollHeight;
      }
    } catch(err) { }
  }

  selectConversation(conv: Conversation) {
    this.activeConv.set(conv);
    conv.unreadCount = 0;
  }

  simulateImageAttach() {
    this.attachedImage.set('https://images.unsplash.com/photo-1584622650111-993a426fbf0a?w=450');
    this.toast.show('تم محاكاة إرفاق صورة العطل.', 'success');
  }

  addEmoji(emoji: string) {
    this.messageText += emoji;
  }

  onSend() {
    const me = this.currentUser();
    const conv = this.activeConv();

    if (!me || !conv) return;

    const text = this.messageText.trim();
    const img = this.attachedImage();

    if (!text && !img) return;

    this.chatService.sendMessage(me.id, conv.otherUser.id, text, img || undefined);

    this.messageText = '';
    this.attachedImage.set('');

    if (conv.otherUser.role === 'worker') {
      this.isTyping.set(true);
      setTimeout(() => {
        this.isTyping.set(false);
      }, 2500);
    }
  }

  isMobileChatView(): boolean {
    if (typeof window !== 'undefined') {
      return window.innerWidth < 768;
    }
    return false;
  }
}

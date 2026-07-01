import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { MockDataService } from '../../core/services/mock-data.service';
import { BookingService } from '../../core/services/booking.service';
import { AuthService } from '../../core/services/auth.service';
import { ToastService } from '../../core/services/toast.service';
import { WorkerProfile } from '../../core/models/interfaces';

@Component({
  selector: 'app-booking',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  template: `
    <div class="max-w-6xl mx-auto px-4 sm:px-6 lg:px-8 py-12 bg-slate-50 dark:bg-slate-900 transition-colors duration-300 min-h-screen">
      
      @if (worker(); as w) {
        
        <!-- Page Title -->
        <div class="text-right space-y-2 mb-10">
          <h1 class="text-3xl font-black text-slate-850 dark:text-white">تأكيد حجز الخدمة</h1>
          <p class="text-sm text-slate-500">أنت على وشك حجز موعد لمعاينة مشكلتك مع الصنايعي.</p>
        </div>

        <div class="grid grid-cols-1 lg:grid-cols-3 gap-8">
          
          <!-- LEFT SUMMARY CARD: BILLING (Col 1) -->
          <div class="lg:col-span-1 order-2 lg:order-1">
            <div class="bg-white dark:bg-slate-950 p-6 rounded-2xl border border-slate-100 dark:border-slate-850 shadow-md text-right space-y-6 sticky top-24">
              
              <h3 class="font-black text-slate-850 dark:text-white text-base pb-3 border-b border-slate-100 dark:border-slate-850">ملخص الحجز والرسوم</h3>

              <!-- Worker Quick Info -->
              <div class="flex items-center gap-3">
                <img [src]="w.avatar" class="w-12 h-12 rounded-xl object-cover border border-slate-100 dark:border-slate-800">
                <div class="text-right flex-grow">
                  <h4 class="text-xs font-black text-slate-800 dark:text-white">{{ w.name }}</h4>
                  <span class="text-[10px] text-slate-450 dark:text-slate-500 font-bold">{{ w.profession }}</span>
                </div>
              </div>

              <!-- Price Breakdown -->
              <div class="space-y-3.5 text-xs pt-2">
                <div class="flex items-center justify-between text-slate-500 dark:text-slate-400">
                  <span class="font-bold">سعر الكشف والمعاينة:</span>
                  <span>{{ w.price }} جنيه</span>
                </div>
                
                <div class="flex items-center justify-between text-slate-500 dark:text-slate-400">
                  <span class="font-bold">رسوم المنصة الإدارية:</span>
                  <span>20 جنيه</span>
                </div>

                <div class="border-t border-slate-100 dark:border-slate-850 pt-3.5 flex items-center justify-between font-black text-sm text-slate-800 dark:text-white">
                  <span>المجموع الإجمالي:</span>
                  <span class="text-primary text-base">{{ w.price + 20 }} جنيه</span>
                </div>
              </div>

              <!-- Payment Note -->
              <div class="p-3 bg-amber-50 dark:bg-amber-950/20 text-amber-800 dark:text-amber-300 rounded-xl text-[11px] leading-relaxed">
                ℹ️ <strong>ملاحظة الدفع:</strong> رسوم كشف المعاينة تُسدد نقداً أو عبر محفظة إلكترونية للصنايعي بعد الزيارة مباشرة.
              </div>

              <button 
                (click)="onSubmit()"
                class="w-full py-3.5 bg-primary hover:bg-primary-hover text-white rounded-xl font-bold shadow-md hover:shadow-lg transition-all text-sm cursor-pointer"
              >
                تأكيد الحجز النهائي
              </button>

            </div>
          </div>

          <!-- RIGHT FORM DETAILS (Col 2-3) -->
          <div class="lg:col-span-2 order-1 lg:order-2 text-right space-y-6">
            
            <div class="bg-white dark:bg-slate-950 p-6 rounded-2xl border border-slate-100 dark:border-slate-850 shadow-sm space-y-6">
              
              <!-- 1. Select Date (Custom Horizontal Calendar Slots) -->
              <div class="space-y-3">
                <h3 class="font-black text-slate-800 dark:text-white text-base">١. حدد تاريخ الزيارة</h3>
                <div class="grid grid-cols-3 sm:grid-cols-5 gap-3 pt-1">
                  @for (slot of dateSlots; track slot.date) {
                    <button 
                      type="button" 
                      (click)="selectedDate = slot.date"
                      [class]="selectedDate === slot.date ? 'border-primary bg-primary/10 text-primary' : 'border-slate-200 dark:border-slate-800 text-slate-700 dark:text-slate-300 hover:border-slate-400'"
                      class="flex flex-col items-center justify-center p-3 rounded-xl border border-solid transition-all cursor-pointer bg-white/20 dark:bg-slate-900/40"
                    >
                      <span class="text-[10px] font-bold text-slate-400 dark:text-slate-500 mb-1">{{ slot.dayName }}</span>
                      <span class="text-base font-black">{{ slot.dayNum }}</span>
                      <span class="text-[9px] font-bold mt-1">{{ slot.monthName }}</span>
                    </button>
                  }
                </div>
              </div>

              <!-- 2. Select Time Slot -->
              <div class="space-y-3 pt-2">
                <h3 class="font-black text-slate-800 dark:text-white text-base">٢. اختر الوقت المفضل</h3>
                <div class="grid grid-cols-1 sm:grid-cols-2 gap-3 pt-1">
                  @for (time of timeSlots; track time) {
                    <button 
                      type="button"
                      (click)="selectedTime = time"
                      [class]="selectedTime === time ? 'border-primary bg-primary/10 text-primary font-extrabold' : 'border-slate-200 dark:border-slate-800 text-slate-600 dark:text-slate-400 hover:border-slate-400'"
                      class="py-3 px-4 rounded-xl border border-solid text-xs text-center transition-all cursor-pointer bg-white/20 dark:bg-slate-900/40"
                    >
                      {{ time }}
                    </button>
                  }
                </div>
              </div>

              <!-- 3. Address Info -->
              <div class="space-y-3 pt-2">
                <h3 class="font-black text-slate-800 dark:text-white text-base">٣. العنوان بالتفصيل</h3>
                <div class="space-y-1">
                  <input 
                    type="text" 
                    [(ngModel)]="address"
                    placeholder="مثال: المعادي - شارع 9 - عمارة 14 شقة 3 الدور الثاني"
                    class="w-full px-4 py-3 rounded-xl border border-slate-200 dark:border-slate-800 bg-slate-50/50 dark:bg-slate-900/40 text-slate-800 dark:text-white outline-none focus:border-primary transition-colors text-right font-semibold"
                  >
                </div>
              </div>

              <!-- 4. Problem Description -->
              <div class="space-y-3 pt-2">
                <h3 class="font-black text-slate-800 dark:text-white text-base">٤. صف المشكلة بالتفصيل</h3>
                <div class="space-y-1">
                  <textarea 
                    [(ngModel)]="description"
                    rows="4" 
                    placeholder="اكتب هنا تفاصيل العطل أو الصيانة المطلوبة لمساعدة الصنايعي على إحضار العدة المناسبة..." 
                    class="w-full px-4 py-3 rounded-xl border border-slate-200 dark:border-slate-800 bg-slate-50/50 dark:bg-slate-900/40 text-slate-800 dark:text-white outline-none focus:border-primary transition-colors text-right resize-none font-semibold"
                  ></textarea>
                </div>
              </div>

              <!-- 5. Mock Photo Upload -->
              <div class="space-y-3 pt-2">
                <h3 class="font-black text-slate-800 dark:text-white text-base">٥. ارفع صوراً للمشكلة (اختياري)</h3>
                <div class="border-2 border-dashed border-slate-200 dark:border-slate-800 rounded-2xl p-6 text-center hover:border-primary transition-colors cursor-pointer bg-slate-50/20 dark:bg-slate-900/20" (click)="simulateUpload()">
                  <div class="text-3xl mb-2">📸</div>
                  <span class="text-xs font-bold text-slate-650 dark:text-slate-350 block mb-1">اسحب الصور أو اضغط هنا للرفع</span>
                  <span class="text-[10px] text-slate-400 dark:text-slate-500 font-bold block">تدعم صيغ JPG، PNG حتى 5 ميجابايت</span>
                  
                  @if (uploadedCount() > 0) {
                    <span class="inline-block mt-3 px-3 py-1 bg-accent/15 text-accent text-xs font-bold rounded-lg animate-slide-up">
                      ✓ تم رفع {{ uploadedCount() }} صور للمشكلة
                    </span>
                  }
                </div>
              </div>

            </div>

          </div>

        </div>

      } @else {
        
        <!-- WORKER NOT FOUND -->
        <div class="max-w-md mx-auto py-24 text-center space-y-6">
          <div class="w-16 h-16 bg-red-105 rounded-full flex items-center justify-center text-3xl mx-auto shadow-md">❌</div>
          <h2 class="text-xl font-black text-slate-800">الصنايعي غير متاح</h2>
          <p class="text-sm text-slate-500">لا يمكن إتمام عملية الحجز لعدم تواجد بيانات الصنايعي.</p>
          <a routerLink="/search" class="inline-block px-6 py-2.5 bg-primary text-white font-bold rounded-xl shadow-md">العودة للبحث</a>
        </div>

      }

    </div>
  `,
  styles: [`
    @keyframes slide-up {
      from { transform: translateY(10px); opacity: 0; }
      to { transform: translateY(0); opacity: 1; }
    }
    .animate-slide-up {
      animation: slide-up 0.25s ease-out forwards;
    }
  `]
})
export default class BookingComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  mockData = inject(MockDataService);
  private bookingService = inject(BookingService);
  private authService = inject(AuthService);
  private toast = inject(ToastService);

  worker = signal<WorkerProfile | null>(null);

  // Form Fields
  selectedDate = '';
  selectedTime = '١٢:٠٠ م - ٠٢:٠٠ م';
  address = '';
  description = '';
  uploadedCount = signal<number>(0);
  uploadedImages: string[] = [];

  // Next 5 Days Slots
  dateSlots: Array<{ date: string; dayName: string; dayNum: number; monthName: string }> = [];

  // Available Time Slots
  timeSlots = [
    '١٠:٠٠ ص - ١٢:٠٠ م',
    '١٢:٠٠ م - ٠٢:٠٠ م',
    '٠٢:٠٠ م - ٠٤:٠٠ م',
    '٠٤:٠٠ م - ٠٦:٠٠ م',
    '٠٦:٠٠ م - ٠٨:٠٠ م'
  ];

  ngOnInit() {
    this.route.paramMap.subscribe(params => {
      const id = params.get('id');
      if (id) {
        const found = this.mockData.workers().find(w => w.id === id);
        if (found) {
          this.worker.set(found);
        }
      }
    });

    this.generateDateSlots();
  }

  generateDateSlots() {
    const days = ['الأحد', 'الاثنين', 'الثلاثاء', 'الأربعاء', 'الخميس', 'الجمعة', 'السبت'];
    const months = [
      'يناير', 'فبراير', 'مارس', 'أبريل', 'مايو', 'يونيو',
      'يوليو', 'أغسطس', 'سبتمبر', 'أكتوبر', 'نوفمبر', 'ديسمبر'
    ];

    const slots = [];
    const today = new Date();

    for (let i = 1; i <= 5; i++) {
      const futureDate = new Date();
      futureDate.setDate(today.getDate() + i);

      const yyyy = futureDate.getFullYear();
      const mm = String(futureDate.getMonth() + 1).padStart(2, '0');
      const dd = String(futureDate.getDate()).padStart(2, '0');

      slots.push({
        date: `${yyyy}-${mm}-${dd}`,
        dayName: days[futureDate.getDay()],
        dayNum: futureDate.getDate(),
        monthName: months[futureDate.getMonth()]
      });
    }

    this.dateSlots = slots;
    this.selectedDate = slots[0].date;
  }

  simulateUpload() {
    this.uploadedCount.update(c => c + 1);
    this.uploadedImages.push('https://images.unsplash.com/photo-1584622650111-993a426fbf0a?w=400');
    this.toast.show('تم رفع الصورة بنجاح.', 'success');
  }

  onSubmit() {
    const w = this.worker();
    const user = this.authService.currentUser();

    if (!w || !user) {
      this.toast.show('يرجى التحقق من تسجيل الدخول وتحديد صنايعي صالح.', 'error');
      return;
    }

    if (!this.address || !this.description) {
      this.toast.show('من فضلك ادخل العنوان التفصيلي ووصف العطل.', 'warning');
      return;
    }

    this.bookingService.createBooking({
      customerId: user.id,
      customerName: user.name,
      workerId: w.id,
      workerName: w.name,
      workerProfession: w.profession,
      date: this.selectedDate,
      time: this.selectedTime,
      address: this.address,
      description: this.description,
      images: this.uploadedImages,
      price: w.price
    });

    this.toast.show('تم إرسال طلب الحجز للصنايعي بنجاح! في انتظار التأكيد.', 'success');
    this.router.navigate(['/customer-dashboard']);
  }
}

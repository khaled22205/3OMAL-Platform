import { Component, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { MockDataService } from '../../core/services/mock-data.service';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  template: `
    <div class="space-y-20 pb-20 bg-slate-50 dark:bg-slate-900 transition-colors duration-300">
      
      <!-- HERO SECTION -->
      <section class="relative overflow-hidden bg-gradient-to-b from-primary/10 via-transparent to-transparent pt-12 pb-24 md:py-32">
        <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 grid grid-cols-1 lg:grid-cols-2 gap-12 items-center">
          
          <div class="space-y-6 text-right order-2 lg:order-1 animate-slide-up">
            <div class="inline-flex items-center gap-2 px-3 py-1 bg-primary/10 text-primary rounded-full text-xs sm:text-sm font-extrabold">
              <span>🚀</span>
              <span>أكبر تجمع للصنايعية المحترفين في مصر</span>
            </div>
            <h1 class="text-4xl sm:text-5xl lg:text-6xl font-black text-slate-850 dark:text-white leading-tight">
              دور على أقرب <span class="text-primary">صنايعي</span><br>في منطقتك
            </h1>
            <p class="text-base sm:text-lg text-slate-600 dark:text-slate-400 max-w-xl leading-relaxed">
              سواء محتاج سباك أو كهربائي أو نجار أو فني تكييف، هتلاقي أفضل الصنايعية بتقييمات حقيقية وأسعار مناسبة.
            </p>
            <div class="flex flex-wrap items-center gap-4 pt-2 justify-start">
              <a routerLink="/search" class="px-8 py-4 bg-primary hover:bg-primary-hover text-white font-bold rounded-2xl shadow-lg shadow-primary/20 hover:shadow-xl hover:translate-y-[-2px] transition-all text-base cursor-pointer">اطلب خدمة الآن</a>
              <a routerLink="/register" class="px-8 py-4 bg-slate-200 dark:bg-slate-800 text-slate-800 dark:text-slate-200 font-bold rounded-2xl border border-slate-350 dark:border-slate-700 hover:bg-slate-300 dark:hover:bg-slate-700 transition-all text-base cursor-pointer">سجل كصنايعي معنا</a>
            </div>
          </div>

          <div class="relative order-1 lg:order-2 flex justify-center items-center">
            <div class="absolute w-72 h-72 rounded-full bg-primary/20 blur-3xl -z-10 animate-pulse"></div>
            <div class="absolute w-60 h-60 rounded-full bg-accent/20 blur-3xl -z-10 right-10 bottom-5"></div>
            <div class="glass p-6 rounded-3xl shadow-2xl border border-white/40 dark:border-white/5 w-full max-w-md relative overflow-hidden transform hover:scale-[1.02] transition-transform duration-500">
              <div class="flex items-center justify-between mb-6">
                <span class="text-xs font-bold text-slate-400">نشاط المنصة اليوم</span>
                <span class="px-2.5 py-1 bg-accent/15 text-accent rounded-lg text-xs font-black">● متصل الآن</span>
              </div>
              <div class="space-y-4">
                <div class="flex items-center gap-4 bg-white/50 dark:bg-slate-950/30 p-3.5 rounded-xl border border-slate-100/50 dark:border-slate-850">
                  <div class="w-10 h-10 rounded-lg bg-orange-100 dark:bg-orange-950/50 flex items-center justify-center text-primary text-xl">🔧</div>
                  <div class="flex flex-col text-right">
                    <span class="text-xs text-slate-450 dark:text-slate-400 font-bold">صنايعي جاهز للعمل</span>
                    <span class="text-lg font-black text-slate-800 dark:text-white">+ 2,450</span>
                  </div>
                </div>
                <div class="flex items-center gap-4 bg-white/50 dark:bg-slate-950/30 p-3.5 rounded-xl border border-slate-100/50 dark:border-slate-850">
                  <div class="w-10 h-10 rounded-lg bg-emerald-100 dark:bg-emerald-950/50 flex items-center justify-center text-accent text-xl">✅</div>
                  <div class="flex flex-col text-right">
                    <span class="text-xs text-slate-450 dark:text-slate-400 font-bold">عملية صيانة ناجحة</span>
                    <span class="text-lg font-black text-slate-800 dark:text-white">+ 18,900</span>
                  </div>
                </div>
                <div class="flex items-center gap-4 bg-white/50 dark:bg-slate-950/30 p-3.5 rounded-xl border border-slate-100/50 dark:border-slate-850">
                  <div class="w-10 h-10 rounded-lg bg-blue-100 dark:bg-blue-950/50 flex items-center justify-center text-blue-500 text-xl">⭐</div>
                  <div class="flex flex-col text-right">
                    <span class="text-xs text-slate-450 dark:text-slate-400 font-bold">تقييم العملاء العام</span>
                    <span class="text-lg font-black text-slate-800 dark:text-white">4.8 / 5.0</span>
                  </div>
                </div>
              </div>
            </div>
          </div>

        </div>
      </section>

      <!-- SEARCH BAR -->
      <section class="max-w-5xl mx-auto px-4 -mt-16 sm:-mt-24 relative z-10 animate-slide-up">
        <div class="bg-white dark:bg-slate-950 p-5 rounded-2xl sm:rounded-3xl shadow-xl border border-slate-150 dark:border-slate-850 grid grid-cols-1 md:grid-cols-4 gap-4">
          
          <div class="flex flex-col gap-1.5 text-right">
            <label class="text-xs font-bold text-slate-500 dark:text-slate-400 pr-1">نوع الخدمة</label>
            <div class="relative">
              <select [(ngModel)]="searchCategory" class="w-full px-4 py-3 bg-slate-50 dark:bg-slate-900 border border-slate-200 dark:border-slate-800 text-slate-800 dark:text-white rounded-xl outline-none focus:border-primary text-right appearance-none font-bold">
                <option value="">كل الخدمات</option>
                @for (cat of mockData.categories(); track cat.id) {
                  <option [value]="cat.id">{{ cat.name }}</option>
                }
              </select>
              <span class="absolute left-4 top-1/2 -translate-y-1/2 pointer-events-none text-slate-400 text-xs">▼</span>
            </div>
          </div>

          <div class="flex flex-col gap-1.5 text-right">
            <label class="text-xs font-bold text-slate-500 dark:text-slate-400 pr-1">المحافظة</label>
            <div class="relative">
              <select [(ngModel)]="searchGov" (change)="onGovChange()" class="w-full px-4 py-3 bg-slate-50 dark:bg-slate-900 border border-slate-200 dark:border-slate-800 text-slate-800 dark:text-white rounded-xl outline-none focus:border-primary text-right appearance-none font-bold">
                <option value="">كل المحافظات</option>
                @for (gov of mockData.governorates; track gov.id) {
                  <option [value]="gov.name">{{ gov.name }}</option>
                }
              </select>
              <span class="absolute left-4 top-1/2 -translate-y-1/2 pointer-events-none text-slate-400 text-xs">▼</span>
            </div>
          </div>

          <div class="flex flex-col gap-1.5 text-right">
            <label class="text-xs font-bold text-slate-500 dark:text-slate-400 pr-1">المنطقة</label>
            <div class="relative">
              <select [(ngModel)]="searchArea" [disabled]="!searchGov" class="w-full px-4 py-3 bg-slate-50 dark:bg-slate-900 border border-slate-200 dark:border-slate-800 text-slate-850 dark:text-white rounded-xl outline-none focus:border-primary text-right appearance-none font-bold disabled:opacity-50">
                <option value="">كل المناطق</option>
                @for (area of availableAreas(); track area) {
                  <option [value]="area">{{ area }}</option>
                }
              </select>
              <span class="absolute left-4 top-1/2 -translate-y-1/2 pointer-events-none text-slate-400 text-xs">▼</span>
            </div>
          </div>

          <div class="flex items-end">
            <button (click)="onSearch()" class="w-full py-3.5 bg-primary hover:bg-primary-hover text-white rounded-xl font-bold shadow-md hover:shadow-lg transition-all text-base flex items-center justify-center gap-2 cursor-pointer">
              <svg class="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2.5">
                <path stroke-linecap="round" stroke-linejoin="round" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
              </svg>
              <span>ابحث الآن</span>
            </button>
          </div>

        </div>
      </section>

      <!-- POPULAR SERVICES -->
      <section class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div class="text-center space-y-3 mb-12">
          <h2 class="text-3xl font-black text-slate-850 dark:text-white">الخدمات الأكثر طلباً</h2>
          <p class="text-sm sm:text-base text-slate-500 max-w-xl mx-auto">اختار الخدمة اللي محتاجها وشوف الصنايعية القريبين منك في ثواني.</p>
        </div>
        <div class="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 gap-6">
          @for (cat of mockData.categories(); track cat.id) {
            <a [routerLink]="['/search']" [queryParams]="{service: cat.id}" class="flex flex-col items-center text-center p-6 rounded-2xl bg-white dark:bg-slate-950 border border-slate-100 dark:border-slate-850 hover:border-primary dark:hover:border-primary hover:shadow-xl hover:translate-y-[-4px] transition-all duration-300 group cursor-pointer">
              <div class="w-14 h-14 rounded-2xl bg-primary/10 text-primary flex items-center justify-center text-2xl mb-4 group-hover:bg-primary group-hover:text-white transition-all duration-300">{{ getCategoryEmoji(cat.id) }}</div>
              <h3 class="text-base font-bold text-slate-800 dark:text-slate-100">{{ cat.name }}</h3>
              <p class="text-[11px] text-slate-400 dark:text-slate-500 mt-2 line-clamp-2 leading-relaxed">{{ cat.description }}</p>
            </a>
          }
        </div>
      </section>

      <!-- FEATURED WORKERS -->
      <section class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div class="flex flex-col sm:flex-row items-center justify-between gap-4 mb-12">
          <div class="text-right space-y-1">
            <h2 class="text-3xl font-black text-slate-850 dark:text-white">صنايعية متميزين</h2>
            <p class="text-sm text-slate-500">أعلى الصنايعية تقييماً ومصداقية من واقع تجارب العملاء الحقيقية.</p>
          </div>
          <a routerLink="/search" class="px-5 py-2.5 bg-slate-100 hover:bg-slate-200 dark:bg-slate-800 dark:hover:bg-slate-700/80 text-slate-700 dark:text-slate-250 font-bold text-xs sm:text-sm rounded-xl border border-slate-200/50 dark:border-slate-750 transition-all cursor-pointer">عرض جميع الصنايعية &larr;</a>
        </div>
        <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
          @for (worker of topWorkers(); track worker.id) {
            <div class="flex flex-col bg-white dark:bg-slate-950 rounded-2xl border border-slate-100 dark:border-slate-850 shadow-sm hover:shadow-xl transition-all duration-300 group relative overflow-hidden">
              <span class="absolute top-3 right-3 px-2 py-0.5 bg-accent text-white rounded-md text-[10px] font-bold z-10">مميز</span>
              <div class="p-5 flex flex-col items-center border-b border-slate-50 dark:border-slate-850 text-center">
                <img [src]="worker.avatar" class="w-20 h-20 rounded-full object-cover border-4 border-slate-50 dark:border-slate-900 shadow-sm">
                <h3 class="text-base font-black text-slate-800 dark:text-white mt-3"><a [routerLink]="['/profile', worker.id]">{{ worker.name }}</a></h3>
                <span class="text-xs font-bold text-primary px-2.5 py-0.5 bg-primary/10 rounded-full mt-1.5">{{ worker.profession }}</span>
                <div class="flex items-center gap-1 mt-3">
                  <span class="text-amber-500 text-sm">★</span>
                  <span class="text-xs font-black text-slate-800 dark:text-slate-100">{{ worker.rating }}</span>
                  <span class="text-[10px] text-slate-400">({{ worker.reviewsCount }} تقييم)</span>
                </div>
              </div>
              <div class="p-5 flex-grow space-y-3.5 text-right text-xs">
                <div class="flex items-center justify-between text-slate-500 dark:text-slate-400"><span class="font-bold">الموقع:</span><span>{{ worker.governorate }}، {{ worker.area }}</span></div>
                <div class="flex items-center justify-between text-slate-500 dark:text-slate-400"><span class="font-bold">سعر الكشف:</span><span class="text-slate-850 dark:text-slate-100 font-extrabold text-sm text-primary">{{ worker.price }} جنيه</span></div>
                <div class="flex items-center justify-between text-slate-500 dark:text-slate-400"><span class="font-bold">الخبرة:</span><span>{{ worker.experience }} سنة</span></div>
              </div>
              <div class="p-5 pt-0">
                <a [routerLink]="['/profile', worker.id]" class="w-full py-2.5 bg-slate-50 hover:bg-primary dark:bg-slate-900/60 dark:hover:bg-primary text-slate-700 hover:text-white dark:text-slate-350 dark:hover:text-white border border-slate-100 hover:border-primary dark:border-slate-800/80 rounded-xl font-bold text-xs flex items-center justify-center transition-all cursor-pointer">عرض الملف الشخصي</a>
              </div>
            </div>
          }
        </div>
      </section>

      <!-- HOW IT WORKS -->
      <section id="how-it-works" class="bg-slate-900 text-white dark:bg-slate-950/80 py-16 md:py-24 transition-colors duration-300">
        <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div class="text-center space-y-3 mb-16">
            <h2 class="text-3xl font-black">إزاي تحجز صنايعي؟</h2>
            <p class="text-sm sm:text-base text-slate-400 max-w-xl mx-auto">خطوات بسيطة وسريعة لحل أي مشكلة صيانة في بيتك.</p>
          </div>
          <div class="grid grid-cols-1 md:grid-cols-3 gap-12 text-center relative">
            <div class="space-y-4 flex flex-col items-center">
              <div class="w-16 h-16 rounded-2xl bg-primary/20 text-primary flex items-center justify-center font-black text-2xl border border-primary/30 shadow-lg">١</div>
              <h3 class="text-lg font-bold">اختار الخدمة</h3>
              <p class="text-sm text-slate-400 max-w-xs leading-relaxed">تصفح الخدمات المتاحة أو ابحث عن المشكلة في منطقتك.</p>
            </div>
            <div class="space-y-4 flex flex-col items-center">
              <div class="w-16 h-16 rounded-2xl bg-accent/20 text-accent flex items-center justify-center font-black text-2xl border border-accent/30 shadow-lg">٢</div>
              <h3 class="text-lg font-bold">اختار الصنايعي</h3>
              <p class="text-sm text-slate-400 max-w-xs leading-relaxed">قارن بين الصنايعية المتاحين بناءً على تقييمات العملاء والأسعار.</p>
            </div>
            <div class="space-y-4 flex flex-col items-center">
              <div class="w-16 h-16 rounded-2xl bg-blue-500/20 text-blue-400 flex items-center justify-center font-black text-2xl border border-blue-500/30 shadow-lg">٣</div>
              <h3 class="text-lg font-bold">احجز الخدمة</h3>
              <p class="text-sm text-slate-400 max-w-xs leading-relaxed">حدد التاريخ والوقت اللي يناسبك وتابع مع الصنايعي.</p>
            </div>
          </div>
        </div>
      </section>

      <!-- TESTIMONIALS -->
      <section class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div class="text-center space-y-3 mb-12">
          <h2 class="text-3xl font-black text-slate-850 dark:text-white">قالوا عن عمال</h2>
          <p class="text-sm text-slate-500 max-w-xl mx-auto">آراء عملاؤنا بعد استخدام الخدمة وتجربة الصنايعية.</p>
        </div>
        <div class="max-w-3xl mx-auto bg-white dark:bg-slate-950 p-8 rounded-2xl sm:rounded-3xl border border-slate-100 dark:border-slate-850 shadow-lg text-center relative overflow-hidden">
          <div class="text-primary text-5xl font-serif leading-none absolute top-4 right-6 opacity-25">”</div>
          <div class="space-y-6">
            <p class="text-base sm:text-lg font-medium text-slate-700 dark:text-slate-300 leading-relaxed max-w-2xl mx-auto">"{{ activeTestimonial().comment }}"</p>
            <div class="flex items-center justify-center gap-3">
              <img [src]="activeTestimonial().avatar" class="w-12 h-12 rounded-full object-cover">
              <div class="text-right">
                <h4 class="text-sm font-bold text-slate-800 dark:text-slate-200">{{ activeTestimonial().name }}</h4>
                <div class="flex items-center gap-1"><span class="text-amber-500 text-xs">★</span><span class="text-[11px] text-slate-400">تقييم ممتاز للخدمة</span></div>
              </div>
            </div>
          </div>
          <div class="flex items-center justify-center gap-4 mt-8">
            <button (click)="prevTestimonial()" class="w-10 h-10 rounded-xl bg-slate-50 hover:bg-slate-100 dark:bg-slate-900 dark:hover:bg-slate-800 text-slate-600 dark:text-slate-350 flex items-center justify-center transition-colors cursor-pointer">&rarr;</button>
            <button (click)="nextTestimonial()" class="w-10 h-10 rounded-xl bg-slate-50 hover:bg-slate-100 dark:bg-slate-900 dark:hover:bg-slate-800 text-slate-600 dark:text-slate-350 flex items-center justify-center transition-colors cursor-pointer">&larr;</button>
          </div>
        </div>
      </section>

      <!-- FAQ -->
      <section id="faq" class="max-w-4xl mx-auto px-4 sm:px-6">
        <div class="text-center space-y-3 mb-12">
          <h2 class="text-3xl font-black text-slate-850 dark:text-white">الأسئلة الشائعة</h2>
          <p class="text-sm text-slate-500">كل الإجابات اللي بتدور عليها بخصوص حجز وضمان الخدمات.</p>
        </div>
        <div class="space-y-4">
          @for (faq of faqs(); track faq.id) {
            <div class="bg-white dark:bg-slate-950 rounded-2xl border border-slate-100 dark:border-slate-850 overflow-hidden transition-all duration-300">
              <button (click)="toggleFaq(faq.id)" class="w-full p-5 flex items-center justify-between text-right font-bold text-slate-800 dark:text-slate-200 hover:bg-slate-50/50 dark:hover:bg-slate-900/30 transition-colors cursor-pointer outline-none">
                <span>{{ faq.question }}</span>
                <span class="text-primary text-xl transform transition-transform duration-300" [class.rotate-45]="faq.open">+</span>
              </button>
              @if (faq.open) {
                <div class="px-5 pb-5 pt-1 text-sm text-slate-500 dark:text-slate-400 border-t border-slate-50 dark:border-slate-900/50 leading-relaxed animate-slide-up">{{ faq.answer }}</div>
              }
            </div>
          }
        </div>
      </section>

    </div>
  `,
  styles: [`
    @keyframes slide-up {
      from { transform: translateY(15px); opacity: 0; }
      to { transform: translateY(0); opacity: 1; }
    }
    .animate-slide-up {
      animation: slide-up 0.4s cubic-bezier(0.16, 1, 0.3, 1) forwards;
    }
  `]
})
export default class Home {
  mockData = inject(MockDataService);
  router = inject(Router);

  searchCategory = '';
  searchGov = '';
  searchArea = '';

  testimonials = [
    { name: 'محمد عبد الله', avatar: 'https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=100', comment: 'كنت محتاج سباك ضروري بالليل متأخر بسبب تسريب ماسورة، دخلت على عمال ولقيت البشمهندس أحمد سعيد متاح.' },
    { name: 'سارة يوسف', avatar: 'https://images.unsplash.com/photo-1494790108377-be9c29b29330?w=100', comment: 'المنصة ممتازة وسهلت عليا صيانة تكييفات البيت. التقييمات بتساعدك تختار وأنت مطمن.' },
    { name: 'عادل إمام', avatar: 'https://images.unsplash.com/photo-1500648767791-00dcc994a43e?w=100', comment: 'شغل عالي ونجار محترف جداً صلحلي الأبواب والدواليب. الدعم الفني كمان تواصل معايا.' }
  ];
  currentTestimonialIndex = signal<number>(0);
  activeTestimonial = computed(() => this.testimonials[this.currentTestimonialIndex()]);

  faqs = signal([
    { id: 1, question: 'هل الأسعار محددة مسبقاً؟', answer: 'سعر كشف الزيارة محدد بوضوح. أما تكلفة المصنعية الإجمالية فيتم الاتفاق عليها بعد المعاينة.', open: false },
    { id: 2, question: 'هل الصنايعية مسجلين ببطاقاتهم الشخصية؟', answer: 'نعم، نقوم بالتحقق من الهوية والفيش والتشبيه لكل صنايعي لضمان الأمان.', open: false },
    { id: 3, question: 'كيف يمكنني سداد قيمة الصيانة؟', answer: 'الدفع يتم نقداً أو عبر المحافظ الإلكترونية للصنايعي بعد العمل.', open: false },
    { id: 4, question: 'ماذا لو حدثت مشكلة بعد مغادرة الصنايعي؟', answer: 'نوفر ضماناً على جميع الخدمات. تواصل مع فريق الدعم وسنحل المشكلة مجاناً.', open: false }
  ]);

  topWorkers = computed(() => this.mockData.workers().slice(0, 4));

  availableAreas = computed(() => {
    const gov = this.mockData.governorates.find(g => g.name === this.searchGov);
    if (gov) { this.searchArea = ''; return gov.areas; }
    return [];
  });

  onGovChange() { this.searchArea = ''; }

  onSearch() {
    this.router.navigate(['/search'], {
      queryParams: { service: this.searchCategory || null, gov: this.searchGov || null, area: this.searchArea || null }
    });
  }

  getCategoryEmoji(id: string): string {
    switch (id) {
      case 'plumber': return '🚰'; case 'electrician': return '⚡'; case 'carpenter': return '🪚';
      case 'ac-tech': return '❄️'; case 'painter': return '🎨'; case 'ceramic': return '🧱';
      case 'blacksmith': return '⚙️'; case 'cleaner': return '🧹'; case 'mover': return '🚚';
      case 'appliances': return '🔌'; default: return '🛠️';
    }
  }

  nextTestimonial() { this.currentTestimonialIndex.update(idx => (idx + 1) % this.testimonials.length); }
  prevTestimonial() { this.currentTestimonialIndex.update(idx => (idx - 1 + this.testimonials.length) % this.testimonials.length); }
  toggleFaq(id: number) { this.faqs.update(items => items.map(f => f.id === id ? { ...f, open: !f.open } : f)); }
}

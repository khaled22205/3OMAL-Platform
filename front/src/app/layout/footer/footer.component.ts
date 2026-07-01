import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-footer',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <footer class="bg-slate-900 text-slate-350 dark:bg-slate-950 border-t border-slate-800 transition-colors duration-300">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-12 md:py-16">
        <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-8 md:gap-12 text-right">
          
          <!-- Column 1: Brand -->
          <div class="flex flex-col gap-4">
            <a routerLink="/" class="flex items-center gap-2">
              <div class="w-9 h-9 rounded-lg bg-primary flex items-center justify-center text-white font-extrabold text-lg">ع</div>
              <span class="text-xl font-black text-white">عمال</span>
            </a>
            <p class="text-xs sm:text-sm text-slate-400 leading-relaxed">
              منصة مصرية رائدة تربطك بأقرب وأمهر الصنايعية في منطقتك. سباكة، كهرباء، نجارة، تكييفات وغيرهم بضمان وتقييمات حقيقية.
            </p>
            <div class="flex items-center gap-3 mt-2">
              <a href="#" class="p-2 rounded-lg bg-slate-800 text-slate-400 hover:bg-primary hover:text-white transition-colors cursor-pointer">
                <svg class="w-5 h-5" fill="currentColor" viewBox="0 0 24 24"><path d="M9 8H7v3h2v9h4v-9h3.6l.4-3H13V6c0-.5.5-1 1-1h2V2h-3a4 4 0 00-4 4v2z"/></svg>
              </a>
              <a href="#" class="p-2 rounded-lg bg-slate-800 text-slate-400 hover:bg-primary hover:text-white transition-colors cursor-pointer">
                <svg class="w-5 h-5" fill="currentColor" viewBox="0 0 24 24"><path d="M23.953 4.57a10 10 0 01-2.825.775 4.958 4.958 0 002.163-2.723c-.951.555-2.005.959-3.127 1.184a4.92 4.92 0 00-8.384 4.482C7.69 8.095 4.067 6.13 1.64 3.162a4.822 4.822 0 00-.666 2.475c0 1.71.87 3.213 2.188 4.096a4.904 4.904 0 01-2.228-.616v.06a4.923 4.923 0 003.946 4.827 4.996 4.996 0 01-2.212.085 4.936 4.936 0 004.604 3.417 9.867 9.867 0 01-6.102 2.105c-.39 0-.779-.023-1.17-.067a13.995 13.995 0 007.557 2.209c9.053 0 13.998-7.496 13.998-13.985 0-.21 0-.42-.015-.63A9.935 9.935 0 0024 4.59z"/></svg>
              </a>
              <a href="#" class="p-2 rounded-lg bg-slate-800 text-slate-400 hover:bg-primary hover:text-white transition-colors cursor-pointer">
                <svg class="w-5 h-5" fill="currentColor" viewBox="0 0 24 24"><path d="M12 2c2.717 0 3.056.01 4.122.06 1.065.05 1.79.217 2.428.465a4.902 4.902 0 011.753 1.14c.488.487.822 1.026 1.141 1.754.248.637.415 1.363.465 2.428.048 1.066.058 1.405.058 4.122 0 2.717-.01 3.056-.058 4.122-.05 1.065-.217 1.79-.465 2.428a4.88 4.88 0 01-1.14 1.753 4.89 4.89 0 01-1.754 1.141c-.637.248-1.363.415-2.428.465-1.066.048-1.405.058-4.122.058-2.717 0-3.056-.01-4.122-.058-1.065-.05-1.79-.217-2.428-.465a4.89 4.89 0 01-1.753-1.14 4.88 4.88 0 01-1.141-1.754c-.248-.637-.415-1.363-.465-2.428C2.013 15.056 2 14.717 2 12c0-2.717.01-3.056.058-4.122.05-1.065.217-1.79.465-2.428a4.88 4.88 0 011.14-1.753 4.89 4.89 0 011.754-1.141c.637-.248 1.363-.415 2.428-.465C8.944 2.013 9.283 2 12 2zm0 4.841A5.16 5.16 0 1012 17.16 5.16 5.16 0 0012 6.84z"/></svg>
              </a>
            </div>
          </div>

          <!-- Column 2: Quick Links -->
          <div>
            <h4 class="text-sm font-bold text-white tracking-wider mb-4 border-r-2 border-primary pr-2">رابط سريعة</h4>
            <ul class="flex flex-col gap-3 text-xs sm:text-sm">
              <li><a routerLink="/" class="hover:text-primary transition-colors">الرئيسية</a></li>
              <li><a routerLink="/search" class="hover:text-primary transition-colors">ابحث عن صنايعي</a></li>
              <li><a href="#how-it-works" class="hover:text-primary transition-colors">كيف تعمل المنصة؟</a></li>
              <li><a routerLink="/register" class="hover:text-primary transition-colors">سجل معنا كصنايعي</a></li>
            </ul>
          </div>

          <!-- Column 3: Services -->
          <div>
            <h4 class="text-sm font-bold text-white tracking-wider mb-4 border-r-2 border-primary pr-2">أهم الخدمات</h4>
            <ul class="flex flex-col gap-3 text-xs sm:text-sm">
              <li><a routerLink="/search" [queryParams]="{service: 'plumber'}" class="hover:text-primary transition-colors">أعمال السباكة</a></li>
              <li><a routerLink="/search" [queryParams]="{service: 'electrician'}" class="hover:text-primary transition-colors">أعمال الكهرباء</a></li>
              <li><a routerLink="/search" [queryParams]="{service: 'carpenter'}" class="hover:text-primary transition-colors">أعمال النجارة</a></li>
              <li><a routerLink="/search" [queryParams]="{service: 'ac-tech'}" class="hover:text-primary transition-colors">صيانة تكييفات</a></li>
            </ul>
          </div>

          <!-- Column 4: Contact Support -->
          <div>
            <h4 class="text-sm font-bold text-white tracking-wider mb-4 border-r-2 border-primary pr-2">الدعم الفني</h4>
            <ul class="flex flex-col gap-3 text-xs sm:text-sm">
              <li class="flex items-center gap-2 justify-end">
                <span>0100 123 4567</span>
                <svg class="w-4 h-4 text-primary" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 5a2 2 0 012-2h3.28a1 1 0 01.94.725l.548 2.2a1 1 0 01-.321.988l-1.305.98a10.582 10.582 0 004.872 4.872l.98-1.305a1 1 0 01.988-.321l2.2.548a1 1 0 01.725.94V19a2 2 0 01-2 2h-1C9.716 21 3 14.284 3 6V5z"/></svg>
              </li>
              <li class="flex items-center gap-2 justify-end">
                <span class="ltr font-semibold">support&#64;omaal.com</span>
                <svg class="w-4 h-4 text-primary" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 8l7.89 5.26a2 2 0 002.22 0L21 8M5 19h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z"/></svg>
              </li>
              <li class="flex items-center gap-2 justify-end">
                <span>القاهرة، جمهورية مصر العربية</span>
                <svg class="w-4 h-4 text-primary" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0zM15 11a3 3 0 11-6 0 3 3 0 016 0z"/></svg>
              </li>
            </ul>
          </div>

        </div>
      </div>

      <div class="bg-slate-950 py-6 border-t border-slate-850 text-center">
        <p class="text-xs sm:text-sm text-slate-500">
          &copy; {{ currentYear }} عمال. جميع الحقوق محفوظة. صُنع بكل حب في مصر.
        </p>
      </div>
    </footer>
  `
})
export class FooterComponent {
  currentYear = new Date().getFullYear();
}

import { Component, inject, signal, computed, OnInit, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { BookingService } from '../../core/services/booking.service';
import { FavoriteService } from '../../core/services/favorite.service';
import { ReviewService } from '../../core/services/review.service';
import { AuthService } from '../../core/services/auth.service';
import { ToastService } from '../../core/services/toast.service';
import { BookingResponse, BookingFilterRequest, BookingStatus } from '../../core/models/booking.models';
import { FavoriteResponse } from '../../core/models/favorite.models';
import { Booking } from '../../core/models/interfaces';
import AiDashboardPageComponent from '../ai-assistant/pages/ai-dashboard-page/ai-dashboard-page.component';

type Tab = 'bookings' | 'profile' | 'favorites' | 'notifications' | 'ai';

interface FavoriteDisplay {
  id: number;
  avatar: string;
  name: string;
  profession: string;
  rating: number;
}

@Component({
  selector: 'app-customer-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, AiDashboardPageComponent],
  template: `
    <div
      class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-10 bg-slate-50 dark:bg-slate-900 transition-colors duration-300 min-h-screen"
    >
      <div class="grid grid-cols-1 lg:grid-cols-4 gap-8">
        <!-- SIDEBAR -->
        <div class="lg:col-span-1">
          <div
            class="bg-white dark:bg-slate-950 p-6 rounded-2xl border border-slate-100 dark:border-slate-850 shadow-sm text-right space-y-6"
          >
            <div
              class="flex items-center gap-3 border-b border-slate-100 dark:border-slate-850 pb-4"
            >
              <img [src]="currentUser()?.avatar" class="w-12 h-12 rounded-xl object-cover" />
              <div class="flex flex-col text-right">
                <span class="text-sm font-extrabold text-slate-800 dark:text-white">{{
                  currentUser()?.name
                }}</span>
                <span class="text-[10px] text-slate-450 font-bold">عميل مسجل</span>
              </div>
            </div>
            <div class="flex flex-col gap-1">
              <button
                (click)="activeTab.set('bookings')"
                [class]="
                  activeTab() === 'bookings'
                    ? 'bg-primary/10 text-primary font-black'
                    : 'text-slate-600 dark:text-slate-400 hover:bg-slate-50 dark:hover:bg-slate-900'
                "
                class="w-full text-right px-4 py-3 rounded-xl text-xs sm:text-sm font-bold transition-all cursor-pointer"
              >
                📅 طلباتي وحجوزاتي
              </button>
              <button
                (click)="activeTab.set('profile')"
                [class]="
                  activeTab() === 'profile'
                    ? 'bg-primary/10 text-primary font-black'
                    : 'text-slate-600 dark:text-slate-400 hover:bg-slate-50 dark:hover:bg-slate-900'
                "
                class="w-full text-right px-4 py-3 rounded-xl text-xs sm:text-sm font-bold transition-all cursor-pointer"
              >
                👤 بياناتي الشخصية
              </button>
              <button
                (click)="activeTab.set('favorites')"
                [class]="
                  activeTab() === 'favorites'
                    ? 'bg-primary/10 text-primary font-black'
                    : 'text-slate-600 dark:text-slate-400 hover:bg-slate-50 dark:hover:bg-slate-900'
                "
                class="w-full text-right px-4 py-3 rounded-xl text-xs sm:text-sm font-bold transition-all cursor-pointer"
              >
                ❤️ المفضلة
              </button>
              <button
                (click)="activeTab.set('notifications')"
                [class]="
                  activeTab() === 'notifications'
                    ? 'bg-primary/10 text-primary font-black'
                    : 'text-slate-600 dark:text-slate-400 hover:bg-slate-50 dark:hover:bg-slate-900'
                "
                class="w-full text-right px-4 py-3 rounded-xl text-xs sm:text-sm font-bold transition-all cursor-pointer flex items-center justify-between"
              >
                <span class="w-2.5 h-2.5 rounded-full bg-red-500"></span><span>🔔 الإشعارات</span>
              </button>
              <button
                (click)="activeTab.set('ai')"
                [class]="
                  activeTab() === 'ai'
                    ? 'bg-primary/10 text-primary font-black'
                    : 'text-slate-600 dark:text-slate-400 hover:bg-slate-50 dark:hover:bg-slate-900'
                "
                class="w-full text-right px-4 py-3 rounded-xl text-xs sm:text-sm font-bold transition-all cursor-pointer"
              >
                🤖 المساعد الذكي
              </button>
            </div>
          </div>
        </div>

        <!-- MAIN CONTENT -->
        <div class="lg:col-span-3 text-right">
          @if (loading()) {
            <div class="flex justify-center items-center py-20">
              <div
                class="w-10 h-10 border-4 border-primary/30 border-t-primary rounded-full animate-spin"
              ></div>
            </div>
          } @else if (error()) {
            <div
              class="bg-red-50 dark:bg-red-950/20 p-8 rounded-2xl border border-red-200 dark:border-red-900 text-center space-y-3"
            >
              <span class="text-4xl block">⚠️</span>
              <p class="text-sm font-bold text-red-600 dark:text-red-400">{{ error() }}</p>
              <button
                (click)="loadData()"
                class="px-5 py-2 bg-red-500 hover:bg-red-600 text-white text-xs font-bold rounded-xl cursor-pointer"
              >
                إعادة المحاولة
              </button>
            </div>
          } @else {
            @if (activeTab() === 'bookings') {
              <div
                class="bg-white dark:bg-slate-950 p-6 rounded-2xl border border-slate-100 dark:border-slate-850 shadow-sm space-y-6"
              >
                <div
                  class="flex justify-between items-center pb-4 border-b border-slate-100 dark:border-slate-850"
                >
                  <span class="text-xs text-slate-400">إدارة ومتابعة طلبات الصيانة</span>
                  <h2 class="text-lg font-black text-slate-850 dark:text-white">طلباتي وحجوزاتي</h2>
                </div>
                <div class="space-y-4">
                  @for (b of userBookings(); track b.id) {
                    <div
                      class="p-5 rounded-2xl border border-slate-100 dark:border-slate-850 bg-slate-50/20 dark:bg-slate-900/10 space-y-4"
                    >
                      <div
                        class="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-3 border-b border-slate-50 dark:border-slate-850 pb-3"
                      >
                        <span
                          [class]="{
                            'bg-amber-100 text-amber-800 dark:bg-amber-950/50 dark:text-amber-300':
                              b.status === 'pending',
                            'bg-blue-100 text-blue-800 dark:bg-blue-950/50 dark:text-blue-300':
                              b.status === 'accepted',
                            'bg-emerald-100 text-emerald-800 dark:bg-emerald-950/50 dark:text-emerald-300':
                              b.status === 'completed',
                            'bg-rose-100 text-rose-800 dark:bg-rose-950/50 dark:text-rose-300':
                              b.status === 'cancelled',
                          }"
                          class="px-3 py-1 rounded-lg text-xs font-black"
                          >{{ getStatusLabel(b.status) }}</span
                        >
                        <div class="flex items-center gap-3">
                          <div class="text-right">
                            <h4 class="text-xs font-black text-slate-800 dark:text-white">
                              صيانة {{ b.workerProfession }} مع {{ b.workerName }}
                            </h4>
                            <span class="text-[10px] text-slate-400 font-bold"
                              >رقم الحجز: {{ b.id }}</span
                            >
                          </div>
                        </div>
                      </div>
                      <div class="grid grid-cols-1 sm:grid-cols-3 gap-4 text-xs">
                        <div class="space-y-1">
                          <span class="font-bold text-slate-400 block">تاريخ الزيارة</span
                          ><span class="text-slate-800 dark:text-slate-200 font-semibold"
                            >{{ b.date }} | {{ b.time }}</span
                          >
                        </div>
                        <div class="space-y-1">
                          <span class="font-bold text-slate-400 block">العنوان</span
                          ><span class="text-slate-700 dark:text-slate-300 leading-normal">{{
                            b.address
                          }}</span>
                        </div>
                        <div class="space-y-1">
                          <span class="font-bold text-slate-400 block">سعر الكشف</span
                          ><span class="text-primary font-black text-sm">{{ b.price }} جنيه</span>
                        </div>
                      </div>
                      <div
                        class="text-xs bg-white dark:bg-slate-950 p-3 rounded-xl border border-slate-100 dark:border-slate-850"
                      >
                        <span class="font-bold text-slate-400 block mb-1">وصف العطل:</span>
                        <p class="text-slate-650 dark:text-slate-350 leading-relaxed">
                          {{ b.description }}
                        </p>
                      </div>
                      <div
                        class="flex flex-wrap items-center gap-2 justify-start pt-2 border-t border-slate-50 dark:border-slate-850/50"
                      >
                        @if (b.status === 'pending') {
                          <button
                            (click)="onCancelBooking(b.id)"
                            class="px-4 py-2 text-xs font-bold text-red-650 hover:bg-red-50 dark:hover:bg-red-950/20 border border-red-200 dark:border-red-900 rounded-xl transition-all cursor-pointer"
                          >
                            إلغاء الطلب
                          </button>
                        }
                        @if (b.status === 'accepted') {
                          <button
                            (click)="onCompleteBooking(b.id)"
                            class="px-4 py-2 text-xs font-bold bg-accent hover:bg-accent-hover text-white rounded-xl shadow-md cursor-pointer"
                          >
                            أكد اكتمال العمل
                          </button>
                        }
                        <a
                          [routerLink]="['/chat']"
                          [queryParams]="{ with: b.workerId }"
                          class="px-4 py-2 text-xs font-bold text-slate-700 dark:text-slate-300 bg-white dark:bg-slate-900 hover:bg-slate-100 dark:hover:bg-slate-800 border border-slate-200 dark:border-slate-800 rounded-xl transition-all cursor-pointer"
                          >مراسلة الصنايعي</a
                        >
                        @if (b.status === 'completed') {
                          <button
                            (click)="onAddReviewSimulate(b)"
                            class="px-4 py-2 text-xs font-bold bg-amber-500 hover:bg-amber-600 text-white rounded-xl shadow-md cursor-pointer"
                          >
                            ⭐ أضف تقييمك
                          </button>
                        }
                      </div>
                    </div>
                  } @empty {
                    <div class="text-center py-16 space-y-4">
                      <span class="text-4xl block">📅</span>
                      <h3 class="text-sm font-bold text-slate-500">لا توجد طلبات حجز مسجلة حالياً</h3>
                      <a
                        routerLink="/search"
                        class="inline-block px-5 py-2 bg-primary text-white text-xs font-bold rounded-lg"
                        >ابحث عن صنايعي واحجز الآن</a
                      >
                    </div>
                  }
                </div>
              </div>
            }

            @if (activeTab() === 'profile') {
              <div
                class="bg-white dark:bg-slate-950 p-6 rounded-2xl border border-slate-100 dark:border-slate-850 shadow-sm space-y-6"
              >
                <div class="pb-4 border-b border-slate-100 dark:border-slate-850">
                  <h2 class="text-lg font-black text-slate-850 dark:text-white">
                    تعديل البيانات الشخصية
                  </h2>
                </div>
                <form (submit)="onSaveProfile()" class="space-y-5 max-w-xl">
                  <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
                    <div class="space-y-1">
                      <label class="text-xs font-bold text-slate-500">الاسم بالكامل</label
                      ><input
                        type="text"
                        [ngModel]="profileName()"
                        (ngModelChange)="profileName.set($event)"
                        name="name"
                        class="w-full px-4 py-2.5 rounded-xl border border-slate-200 dark:border-slate-800 bg-slate-50/50 dark:bg-slate-900 text-xs sm:text-sm outline-none text-right font-semibold"
                      />
                    </div>
                    <div class="space-y-1">
                      <label class="text-xs font-bold text-slate-500">رقم الهاتف</label
                      ><input
                        type="text"
                        [ngModel]="profilePhone()"
                        (ngModelChange)="profilePhone.set($event)"
                        name="phone"
                        class="w-full px-4 py-2.5 rounded-xl border border-slate-200 dark:border-slate-800 bg-slate-50/50 dark:bg-slate-900 text-xs sm:text-sm outline-none text-right font-semibold"
                      />
                    </div>
                  </div>
                  <div class="space-y-1">
                    <label class="text-xs font-bold text-slate-500">البريد الإلكتروني</label
                    ><input
                      type="email"
                      [ngModel]="profileEmail()"
                      (ngModelChange)="profileEmail.set($event)"
                      name="email"
                      class="w-full px-4 py-2.5 rounded-xl border border-slate-200 dark:border-slate-800 bg-slate-50/50 dark:bg-slate-900 text-xs sm:text-sm outline-none text-right font-semibold"
                    />
                  </div>
                  <button
                    type="submit"
                    class="px-6 py-2.5 bg-primary hover:bg-primary-hover text-white rounded-xl font-bold text-xs sm:text-sm shadow-md cursor-pointer"
                  >
                    حفظ التعديلات
                  </button>
                </form>
              </div>
            }

            @if (activeTab() === 'favorites') {
              <div
                class="bg-white dark:bg-slate-950 p-6 rounded-2xl border border-slate-100 dark:border-slate-850 shadow-sm space-y-6"
              >
                <div class="pb-4 border-b border-slate-100 dark:border-slate-850">
                  <h2 class="text-lg font-black text-slate-850 dark:text-white">الصنايعية المفضلة</h2>
                </div>
                <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
                  @for (fav of favoriteWorkers(); track fav.id) {
                    <div
                      class="flex items-center gap-3 p-4 rounded-xl border border-slate-150 dark:border-slate-850 bg-slate-50/30 dark:bg-slate-900/30"
                    >
                      <img [src]="fav.avatar" class="w-14 h-14 rounded-xl object-cover border" />
                      <div class="text-right flex-grow">
                        <h4 class="text-xs sm:text-sm font-black text-slate-850 dark:text-white">
                          {{ fav.name }}
                        </h4>
                        <span
                          class="text-[10px] text-primary font-bold px-2 py-0.5 bg-primary/10 rounded mt-1 inline-block"
                          >{{ fav.profession }}</span
                        >
                        <div class="flex items-center gap-1 mt-1 text-xs">
                          <span class="text-amber-500">★</span
                          ><span class="font-bold">{{ fav.rating }}</span>
                        </div>
                      </div>
                      <a
                        [routerLink]="['/profile', fav.id]"
                        class="px-3 py-1.5 bg-primary text-white text-[10px] font-bold rounded-lg"
                        >عرض</a
                      >
                    </div>
                  } @empty {
                    <p class="text-xs text-slate-400 text-center py-6 col-span-2">
                      لا يوجد صنايعية في المفضلة.
                    </p>
                  }
                </div>
              </div>
            }

            @if (activeTab() === 'notifications') {
              <div
                class="bg-white dark:bg-slate-950 p-6 rounded-2xl border border-slate-100 dark:border-slate-850 shadow-sm space-y-6"
              >
                <div class="pb-4 border-b border-slate-100 dark:border-slate-850">
                  <h2 class="text-lg font-black text-slate-850 dark:text-white">
                    الإشعارات والتنبيهات
                  </h2>
                </div>
                <div class="space-y-3">
                  <div
                    class="p-4 bg-emerald-50 dark:bg-emerald-950/20 text-emerald-800 dark:text-emerald-350 rounded-xl border border-emerald-100 dark:border-emerald-900 text-xs flex justify-between items-center gap-4"
                  >
                    <span class="text-[10px] text-slate-400">منذ ساعة</span>
                    <p class="font-semibold leading-relaxed">
                      ✓ تم تأكيد طلب الحجز مع الصنايعي <strong>أحمد سعيد</strong> للغد.
                    </p>
                  </div>
                  <div
                    class="p-4 bg-slate-50 dark:bg-slate-900/30 text-slate-700 dark:text-slate-350 rounded-xl border border-slate-150 dark:border-slate-800 text-xs flex justify-between items-center gap-4"
                  >
                    <span class="text-[10px] text-slate-400">منذ يومين</span>
                    <p class="font-semibold leading-relaxed">
                      📩 استلمت رسالة شات جديدة من الكهربائي محمود الصاوي.
                    </p>
                  </div>
                </div>
              </div>
            }
            @if (activeTab() === 'ai') {
              <app-ai-dashboard-page />
            }
          }
        </div>
      </div>
    </div>
  `,
})
export default class CustomerDashboard implements OnInit {
  private bookingService = inject(BookingService);
  private favoriteService = inject(FavoriteService);
  private reviewService = inject(ReviewService);
  authService = inject(AuthService);
  private toast = inject(ToastService);
  private destroyRef = inject(DestroyRef);

  activeTab = signal<Tab>('bookings');
  loading = signal(true);
  error = signal<string | null>(null);

  allBookings = signal<BookingResponse[]>([]);
  favoriteItems = signal<FavoriteResponse[]>([]);

  currentUser = this.authService.currentUser;

  profileName = signal(this.currentUser()?.name || '');
  profilePhone = signal(this.currentUser()?.phone || '');
  profileEmail = signal(this.currentUser()?.email || '');

  userBookings = computed<Booking[]>(() => {
    return this.allBookings().map((b) => ({
      id: String(b.id),
      customerId: String(b.customerId),
      customerName: b.customerName,
      workerId: String(b.workerProfileId),
      workerName: b.workerName,
      workerProfession: b.serviceName || '',
      date: this.formatDate(b.scheduledAt),
      time: this.formatTime(b.scheduledAt),
      address: b.address || '',
      description: b.notes || '',
      images: [],
      status: this.mapOldStatus(b.status),
      price: b.totalPrice,
      createdAt: b.createdAt,
    }));
  });

  favoriteWorkers = computed<FavoriteDisplay[]>(() => {
    return this.favoriteItems().map((f) => ({
      id: f.workerProfileId || f.id,
      avatar:
        f.workerPhoto ||
        'https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?w=100',
      name: f.workerName || 'صنايعي',
      profession: f.serviceName || 'مهني',
      rating: f.workerRating || 0,
    }));
  });

  ngOnInit() {
    this.loadData();
  }

  loadData() {
    this.loading.set(true);
    this.error.set(null);

    const filter: BookingFilterRequest = { page: 1, pageSize: 50 };
    this.bookingService
      .getMyBookings(filter)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (result) => {
          this.allBookings.set(result.items || []);
          this.loading.set(false);
        },
        error: () => {
          this.error.set('فشل تحميل الحجوزات');
          this.loading.set(false);
          this.toast.show('فشل تحميل الحجوزات', 'error');
        },
      });

    this.favoriteService
      .getAll(1, 20)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (result) => this.favoriteItems.set(result.items || []),
      });
  }

  getStatusLabel(status: Booking['status']): string {
    switch (status) {
      case 'pending':
        return 'في انتظار الموافقة';
      case 'accepted':
        return 'مؤكد - بانتظار الزيارة';
      case 'completed':
        return 'مكتملة';
      case 'cancelled':
        return 'ملغية';
    }
  }

  onCancelBooking(id: string) {
    const numericId = parseInt(id, 10);
    if (isNaN(numericId)) return;
    this.bookingService
      .cancel(numericId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.toast.show('تم إلغاء حجز الصيانة بنجاح.', 'info');
          this.refreshBookings();
        },
        error: () => this.toast.show('فشل إلغاء الحجز', 'error'),
      });
  }

  onCompleteBooking(id: string) {
    const numericId = parseInt(id, 10);
    if (isNaN(numericId)) return;
    this.bookingService
      .completeJob(numericId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.toast.show('تم تأكيد اكتمال الصيانة!', 'success');
          this.refreshBookings();
        },
        error: () => this.toast.show('فشل تأكيد اكتمال الصيانة', 'error'),
      });
  }

  onSaveProfile() {
    const user = this.authService.user();
    if (user) {
      this.authService.user.set({
        ...user,
        firstName: this.profileName(),
        phoneNumber: this.profilePhone(),
        email: this.profileEmail(),
      });
    }
    this.toast.show('تم تحديث بياناتك الشخصية بنجاح.', 'success');
  }

  onAddReviewSimulate(booking: Booking) {
    const rating = prompt('تقييمك (1-5):', '5');
    if (!rating) return;
    const comment = prompt('تعليقك:', 'شغل احترافي وسريع.');
    if (comment === null) return;

    const bookingId = parseInt(booking.id, 10);
    if (isNaN(bookingId)) {
      this.toast.show('فشل تقديم التقييم', 'error');
      return;
    }

    this.reviewService
      .create({
        bookingId,
        rating: Math.min(5, Math.max(1, parseInt(rating, 10) || 5)),
        comment: comment || undefined,
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => this.toast.show('تم تقديم تقييمك بنجاح!', 'success'),
        error: () => this.toast.show('فشل تقديم التقييم', 'error'),
      });
  }

  private refreshBookings() {
    const filter: BookingFilterRequest = { page: 1, pageSize: 50 };
    this.bookingService
      .getMyBookings(filter)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (result) => this.allBookings.set(result.items || []),
      });
  }

  private formatDate(iso: string): string {
    if (!iso) return '';
    return new Date(iso).toLocaleDateString('ar-EG');
  }

  private formatTime(iso: string): string {
    if (!iso) return '';
    return new Date(iso).toLocaleTimeString('ar-EG', { hour: '2-digit', minute: '2-digit' });
  }

  private mapOldStatus(s: BookingStatus): 'pending' | 'accepted' | 'completed' | 'cancelled' {
    switch (s) {
      case 'Pending':
        return 'pending';
      case 'Accepted':
      case 'Scheduled':
      case 'OnTheWay':
      case 'Started':
      case 'Paused':
        return 'accepted';
      case 'Completed':
        return 'completed';
      default:
        return 'cancelled';
    }
  }
}

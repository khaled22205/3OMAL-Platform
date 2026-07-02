import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { BaseApiService } from './base-api.service';
import { PaymentResponse, ProcessPaymentRequest } from '../models/payment.models';

@Injectable({ providedIn: 'root' })
export class PaymentService extends BaseApiService {
  getByBookingId(bookingId: number): Observable<PaymentResponse> {
    return this.get<PaymentResponse>(`/payments/booking/${bookingId}`);
  }

  processPayment(request: ProcessPaymentRequest): Observable<PaymentResponse> {
    return this.post<PaymentResponse>('/payments/process', request);
  }

  refund(bookingId: number): Observable<{ message: string }> {
    return this.post<{ message: string }>(`/payments/${bookingId}/refund`, {});
  }
}

export interface PaymentResponse {
  id: number;
  bookingId: number;
  amount: number;
  commissionAmount: number;
  paymentMethod: string;
  status: string;
  completedAt?: string;
  transactionReference?: string;
}

export interface ProcessPaymentRequest {
  bookingId: number;
  paymentMethod: string;
}

export interface WithdrawalRequest {
  amount: number;
  bankAccount: string;
}

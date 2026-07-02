export type BookingStatus =
  | 'Pending'
  | 'Accepted'
  | 'Rejected'
  | 'Scheduled'
  | 'OnTheWay'
  | 'Started'
  | 'Paused'
  | 'Completed'
  | 'Cancelled'
  | 'Expired';

export interface CreateBookingRequest {
  workerProfileId: number;
  workerServiceId?: number;
  scheduledAt: string;
  address?: string;
  notes?: string;
}

export interface BookingResponse {
  id: number;
  customerId: number;
  customerName: string;
  workerProfileId: number;
  workerName: string;
  workerServiceId?: number;
  serviceName?: string;
  status: BookingStatus;
  scheduledAt: string;
  address?: string;
  notes?: string;
  totalPrice: number;
  commissionAmount: number;
  createdAt: string;
  startedAt?: string;
  completedAt?: string;
  cancelledAt?: string;
  cancellationReason?: string;
}

export interface UpdateBookingStatusRequest {
  status: string;
  cancellationReason?: string;
}

export interface BookingFilterRequest {
  status?: string;
  workerProfileId?: number;
  customerId?: number;
  fromDate?: string;
  toDate?: string;
  page: number;
  pageSize: number;
}

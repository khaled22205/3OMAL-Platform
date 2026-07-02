export interface CreateReviewRequest {
  bookingId: number;
  rating: number;
  comment?: string;
}

export interface UpdateReviewRequest {
  rating: number;
  comment?: string;
}

export interface ReviewResponse {
  id: number;
  bookingId: number;
  customerId: number;
  customerName: string;
  customerPhoto?: string;
  workerProfileId: number;
  workerName: string;
  rating: number;
  comment?: string;
  workerReply?: string;
  isEdited: boolean;
  createdAt: string;
}

export interface WorkerReplyRequest {
  reply: string;
}

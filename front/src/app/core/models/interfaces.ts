export interface User {
  id: string;
  name: string;
  email: string;
  phone: string;
  role: 'client' | 'worker';
  avatar: string;
  createdAt: string;
}

export interface Review {
  id: string;
  reviewerName: string;
  reviewerAvatar: string;
  rating: number;
  comment: string;
  date: string;
}

export interface WorkerProfile {
  id: string;
  name: string;
  email: string;
  phone: string;
  avatar: string;
  profession: string;
  professionId: string;
  governorate: string;
  area: string;
  experience: number;
  rating: number;
  reviewsCount: number;
  bio: string;
  price: number;
  portfolio: string[];
  reviews: Review[];
  availableAreas: string[];
}

export interface Category {
  id: string;
  name: string;
  englishName: string;
  icon: string;
  description: string;
}

export interface Booking {
  id: string;
  customerId: string;
  customerName: string;
  workerId: string;
  workerName: string;
  workerProfession: string;
  date: string;
  time: string;
  address: string;
  description: string;
  images: string[];
  status: 'pending' | 'accepted' | 'completed' | 'cancelled';
  price: number;
  createdAt: string;
}

export interface Message {
  id: string;
  senderId: string;
  receiverId: string;
  content: string;
  image?: string;
  timestamp: string;
  read: boolean;
}

export interface Conversation {
  id: string;
  otherUser: {
    id: string;
    name: string;
    avatar: string;
    role: 'client' | 'worker';
    profession?: string;
  };
  lastMessage: Message;
  unreadCount: number;
}

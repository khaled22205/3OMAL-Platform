import { Injectable, signal } from '@angular/core';
import { Category, WorkerProfile, Booking, Message, Conversation } from '../models/interfaces';

@Injectable({ providedIn: 'root' })
export class MockDataService {
  categories = signal<Category[]>([
    { id: 'plumber', name: 'سباك', englishName: 'Plumber', icon: 'Pipette', description: 'تركيب وصيانة السباكة والمواسير والصرف الصحي.' },
    { id: 'electrician', name: 'كهربائي', englishName: 'Electrician', icon: 'Zap', description: 'تأسيس وصيانة أعمال الكهرباء والإضاءة المنزلية.' },
    { id: 'carpenter', name: 'نجار', englishName: 'Carpenter', icon: 'Hammer', description: 'تصنيع وتصليح وتركيب الأثاث والأبواب والشبابيك.' },
    { id: 'ac-tech', name: 'فني تكييف', englishName: 'AC Tech', icon: 'Wind', description: 'تركيب وصيانة وشحن فريون أجهزة التكييف.' },
    { id: 'painter', name: 'نقاش', englishName: 'Painter', icon: 'Paintbrush', description: 'أعمال الدهانات والديكورات وورق الحائط.' },
    { id: 'ceramic', name: 'سيراميك', englishName: 'Ceramic', icon: 'Grid', description: 'تركيب السيراميك والبورسلين والرخام للارضيات والجدران.' },
    { id: 'blacksmith', name: 'حداد', englishName: 'Blacksmith', icon: 'Wrench', description: 'أعمال الكريتال والبوابات والحمايات الحديدية.' },
    { id: 'cleaner', name: 'تنظيف منازل', englishName: 'Cleaner', icon: 'Sparkles', description: 'تنظيف وتطهير الشقق والفيلات والسجاد والانتريهات.' },
    { id: 'mover', name: 'نقل عفش', englishName: 'Mover', icon: 'Truck', description: 'فك وتغليف ونقل وتركيب الاثاث بأمان.' },
    { id: 'appliances', name: 'صيانة أجهزة منزلية', englishName: 'Appliances', icon: 'Cpu', description: 'تصليح الغسالات، الثلاجات، البوتجازات والميكروويف.' }
  ]);

  governorates = [
    { id: 'cairo', name: 'القاهرة', areas: ['المعادي', 'التجمع الخامس', 'مصر الجديدة', 'مدينة نصر', 'شبرا', 'حلوان', 'وسط البلد'] },
    { id: 'giza', name: 'الجيزة', areas: ['المهندسين', 'الدقي', 'الهرم', 'أكتوبر', 'الشيخ زايد', 'فيصل', 'العجوزة'] },
    { id: 'alexandria', name: 'الإسكندرية', areas: ['سموحة', 'سيدي بشر', 'المنتزة', 'محرم بك', 'لوران', 'العصافرة', 'الرمل'] }
  ];

  private portfolioImages = [
    'https://images.unsplash.com/photo-1584622650111-993a426fbf0a?w=500&auto=format&fit=crop&q=60',
    'https://images.unsplash.com/photo-1621905251189-08b45d6a269e?w=500&auto=format&fit=crop&q=60',
    'https://images.unsplash.com/photo-1504307651254-35680f356dfd?w=500&auto=format&fit=crop&q=60',
    'https://images.unsplash.com/photo-1581094288338-2314dddb7ecc?w=500&auto=format&fit=crop&q=60',
    'https://images.unsplash.com/photo-1600585154340-be6161a56a0c?w=500&auto=format&fit=crop&q=60'
  ];

  workers = signal<WorkerProfile[]>([
    {
      id: 'w1', name: 'أحمد سعيد', email: 'ahmed.plumber@omaal.com', phone: '01012345678',
      avatar: 'https://images.unsplash.com/photo-1540569014015-19a7be504e3a?w=150&auto=format&fit=crop&q=80',
      profession: 'سباك', professionId: 'plumber', governorate: 'القاهرة', area: 'المعادي',
      experience: 12, rating: 4.8, reviewsCount: 38, price: 150,
      bio: 'سباك محترف خبرة أكثر من 12 سنة في تأسيس وصيانة سباكة الفلل والشقق.',
      portfolio: [this.portfolioImages[0], this.portfolioImages[4]],
      availableAreas: ['المعادي', 'وسط البلد', 'حلوان', 'مدينة نصر'],
      reviews: [
        { id: 'r1', reviewerName: 'محمد علي', reviewerAvatar: 'https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=100', rating: 5, comment: 'راجل محترم جداً وجه في الميعاد بالظبط وحل المشكلة بسرعة.', date: '2026-06-25' },
        { id: 'r2', reviewerName: 'سارة أحمد', reviewerAvatar: 'https://images.unsplash.com/photo-1494790108377-be9c29b29330?w=100', rating: 4.5, comment: 'شغل نضيف جداً ومحترف في عمله.', date: '2026-06-18' }
      ]
    },
    {
      id: 'w2', name: 'محمود الصاوي', email: 'mahmoud.electric@omaal.com', phone: '01198765432',
      avatar: 'https://images.unsplash.com/photo-1500648767791-00dcc994a43e?w=150&auto=format&fit=crop&q=80',
      profession: 'كهربائي', professionId: 'electrician', governorate: 'القاهرة', area: 'التجمع الخامس',
      experience: 8, rating: 4.9, reviewsCount: 52, price: 200,
      bio: 'متخصص في تشطيبات الكهرباء الحديثة وتركيب النجف والاسبوتات.',
      portfolio: [this.portfolioImages[1]],
      availableAreas: ['التجمع الخامس', 'مصر الجديدة', 'مدينة نصر'],
      reviews: [
        { id: 'r3', reviewerName: 'خالد يوسف', reviewerAvatar: 'https://images.unsplash.com/photo-1472099645785-5658abf4ff4e?w=100', rating: 5, comment: 'ما شاء الله عليه شغله دقيق جداً.', date: '2026-06-29' }
      ]
    },
    {
      id: 'w3', name: 'إبراهيم النجار', email: 'ibrahim.carpenter@omaal.com', phone: '01234567890',
      avatar: 'https://images.unsplash.com/photo-1519085360753-af0119f7cbe7?w=150&auto=format&fit=crop&q=80',
      profession: 'نجار', professionId: 'carpenter', governorate: 'الجيزة', area: 'المهندسين',
      experience: 15, rating: 4.7, reviewsCount: 29, price: 180,
      bio: 'تخصص نجارة باب وشباك وتصليح غرف النوم والمطابخ.',
      portfolio: [this.portfolioImages[2]],
      availableAreas: ['المهندسين', 'الدقي', 'العجوزة', 'أكتوبر'],
      reviews: [
        { id: 'r4', reviewerName: 'حسين فهمي', reviewerAvatar: 'https://images.unsplash.com/photo-1500648767791-00dcc994a43e?w=100', rating: 4, comment: 'صلحلي باب الشقة والدولاب، شغله متين.', date: '2026-06-12' }
      ]
    },
    {
      id: 'w4', name: 'هاني تكييف', email: 'hani.ac@omaal.com', phone: '01511223344',
      avatar: 'https://images.unsplash.com/photo-1539571696357-5a69c17a67c6?w=150&auto=format&fit=crop&q=80',
      profession: 'فني تكييف', professionId: 'ac-tech', governorate: 'الجيزة', area: 'أكتوبر',
      experience: 6, rating: 4.6, reviewsCount: 22, price: 250,
      bio: 'فني متخصص في صيانة وتركيب جميع أنواع التكييفات.',
      portfolio: [this.portfolioImages[3]],
      availableAreas: ['أكتوبر', 'الشيخ زايد', 'الهرم'],
      reviews: [
        { id: 'r5', reviewerName: 'عمر مصطفى', reviewerAvatar: 'https://images.unsplash.com/photo-1519345182560-3f2917c472ef?w=100', rating: 5, comment: 'نضف التكييفات والشغل كان ممتاز.', date: '2026-06-27' }
      ]
    },
    {
      id: 'w5', name: 'كريم النقاش', email: 'karim.painter@omaal.com', phone: '01066778899',
      avatar: 'https://images.unsplash.com/photo-1566492031773-4f4e44671857?w=150&auto=format&fit=crop&q=80',
      profession: 'نقاش', professionId: 'painter', governorate: 'الإسكندرية', area: 'سموحة',
      experience: 10, rating: 4.9, reviewsCount: 45, price: 130,
      bio: 'أحدث الديكورات وأوراق الحائط ودهانات القطيفة والثرى دي.',
      portfolio: [this.portfolioImages[4], this.portfolioImages[2]],
      availableAreas: ['سموحة', 'سيدي بشر', 'الرمل', 'لوران'],
      reviews: [
        { id: 'r6', reviewerName: 'رانيا فريد', reviewerAvatar: 'https://images.unsplash.com/photo-1534528741775-53994a69daeb?w=100', rating: 5, comment: 'كريم دهان ممتاز صبغ شقتي كلها في ٣ أيام.', date: '2026-06-20' }
      ]
    },
    {
      id: 'w6', name: 'حسن سيراميك', email: 'hassan.ceramic@omaal.com', phone: '01099887766',
      avatar: 'https://images.unsplash.com/photo-1618015358954-115ef1ed1815?w=150&auto=format&fit=crop&q=80',
      profession: 'سيراميك', professionId: 'ceramic', governorate: 'القاهرة', area: 'مدينة نصر',
      experience: 14, rating: 4.8, reviewsCount: 31, price: 170,
      bio: 'فني تركيب سيراميك وبورسلين وبورسلين هندي ليزر.',
      portfolio: [this.portfolioImages[0]],
      availableAreas: ['مدينة نصر', 'مصر الجديدة', 'التجمع الخامس'],
      reviews: [
        { id: 'r7', reviewerName: 'طارق شوقي', reviewerAvatar: 'https://images.unsplash.com/photo-1544005313-94ddf0286df2?w=100', rating: 5, comment: 'شغله مظبوط بالملي.', date: '2026-06-22' }
      ]
    },
    {
      id: 'w7', name: 'أشرف حداد', email: 'ashraf.iron@omaal.com', phone: '01122334455',
      avatar: 'https://images.unsplash.com/photo-1552058544-f2b08422138a?w=150&auto=format&fit=crop&q=80',
      profession: 'حداد', professionId: 'blacksmith', governorate: 'الجيزة', area: 'الهرم',
      experience: 11, rating: 4.5, reviewsCount: 18, price: 120,
      bio: 'تفصيل وتركيب البوابات الحديد وحمايات الشبابيك.',
      portfolio: [this.portfolioImages[1]],
      availableAreas: ['الهرم', 'فيصل', 'الدقي', 'المهندسين'],
      reviews: [
        { id: 'r8', reviewerName: 'أدهم سليمان', reviewerAvatar: 'https://images.unsplash.com/photo-1506794778202-cad84cf45f1d?w=100', rating: 4, comment: 'عملي حماية ممتازة لشبابيك الشقة.', date: '2026-06-05' }
      ]
    },
    {
      id: 'w8', name: 'أميرة كلين', email: 'amira.clean@omaal.com', phone: '01255443322',
      avatar: 'https://images.unsplash.com/photo-1573496359142-b8d87734a5a2?w=150&auto=format&fit=crop&q=80',
      profession: 'تنظيف منازل', professionId: 'cleaner', governorate: 'القاهرة', area: 'مصر الجديدة',
      experience: 5, rating: 4.7, reviewsCount: 40, price: 100,
      bio: 'خدمات تنظيف شقق سكنية ومكاتب وفيلات بأفضل المواد المنظفة.',
      portfolio: [this.portfolioImages[3]],
      availableAreas: ['مصر الجديدة', 'مدينة نصر', 'التجمع الخامس'],
      reviews: [
        { id: 'r9', reviewerName: 'منى زكي', reviewerAvatar: 'https://images.unsplash.com/photo-1544005313-94ddf0286df2?w=100', rating: 5, comment: 'فريق العمل محترم والأمانة عالية جداً.', date: '2026-06-24' }
      ]
    }
  ]);

  bookings = signal<Booking[]>([
    {
      id: 'b-101', customerId: 'c1', customerName: 'يوسف محمد',
      workerId: 'w1', workerName: 'أحمد سعيد', workerProfession: 'سباك',
      date: '2026-07-02', time: '12:00 - 02:00 م',
      address: 'المعادي - شارع 9 - عمارة 45 شقة 3',
      description: 'عندي تسريب مية تحت حوض المطبخ.',
      images: ['https://images.unsplash.com/photo-1584622650111-993a426fbf0a?w=150'],
      status: 'pending', price: 150, createdAt: '2026-07-01T10:00:00Z'
    },
    {
      id: 'b-102', customerId: 'c1', customerName: 'يوسف محمد',
      workerId: 'w2', workerName: 'محمود الصاوي', workerProfession: 'كهربائي',
      date: '2026-06-28', time: '04:00 - 06:00 م',
      address: 'التجمع الخامس - حي الياسمين - فيلا 12',
      description: 'تركيب نجفة صالون و 10 اسبوتات ليد.',
      images: [], status: 'completed', price: 200, createdAt: '2026-06-27T08:30:00Z'
    }
  ]);

  messages = signal<Message[]>([
    { id: 'm1', senderId: 'c1', receiverId: 'w1', content: 'السلام عليكم يا فنان، متاح تيجي بكرة تبص على مشكلة السباكة؟', timestamp: '2026-07-01T10:15:00Z', read: true },
    { id: 'm2', senderId: 'w1', receiverId: 'c1', content: 'وعليكم السلام يا باشا، أه متاح بكرة إن شاء الله.', timestamp: '2026-07-01T10:17:00Z', read: true },
    { id: 'm3', senderId: 'c1', receiverId: 'w1', content: 'تمام جداً يناسبني الساعة ١٢.', timestamp: '2026-07-01T10:20:00Z', read: false }
  ]);

  conversations = signal<Conversation[]>([
    {
      id: 'conv1',
      otherUser: { id: 'w1', name: 'أحمد سعيد', avatar: 'https://images.unsplash.com/photo-1540569014015-19a7be504e3a?w=150&auto=format&fit=crop&q=80', role: 'worker', profession: 'سباك' },
      lastMessage: { id: 'm3', senderId: 'c1', receiverId: 'w1', content: 'تمام جداً يناسبني الساعة ١٢.', timestamp: '2026-07-01T10:20:00Z', read: false },
      unreadCount: 0
    },
    {
      id: 'conv2',
      otherUser: { id: 'w2', name: 'محمود الصاوي', avatar: 'https://images.unsplash.com/photo-1500648767791-00dcc994a43e?w=150&auto=format&fit=crop&q=80', role: 'worker', profession: 'كهربائي' },
      lastMessage: { id: 'm_prev', senderId: 'w2', receiverId: 'c1', content: 'الشغل كله تمام يا فندم.', timestamp: '2026-06-28T18:00:00Z', read: true },
      unreadCount: 0
    }
  ]);

  addBooking(bookingData: Omit<Booking, 'id' | 'status' | 'createdAt'>) {
    const newBooking: Booking = {
      ...bookingData,
      id: 'b-' + Math.floor(1000 + Math.random() * 9000),
      status: 'pending',
      createdAt: new Date().toISOString()
    };
    this.bookings.update(curr => [newBooking, ...curr]);
    return newBooking;
  }

  updateBookingStatus(bookingId: string, status: Booking['status']) {
    this.bookings.update(curr =>
      curr.map(b => b.id === bookingId ? { ...b, status } : b)
    );
  }

  sendMessage(senderId: string, receiverId: string, content: string, image?: string) {
    const newMessage: Message = {
      id: 'msg-' + Date.now(), senderId, receiverId, content, image,
      timestamp: new Date().toISOString(), read: false
    };
    this.messages.update(curr => [...curr, newMessage]);
    this.conversations.update(convs => {
      const existing = convs.find(c => c.otherUser.id === (senderId === 'c1' ? receiverId : senderId));
      if (existing) {
        return convs.map(c => c.otherUser.id === existing.otherUser.id ? {
          ...c, lastMessage: newMessage,
          unreadCount: senderId === 'c1' ? 0 : c.unreadCount + 1
        } : c).sort((a,b) => new Date(b.lastMessage.timestamp).getTime() - new Date(a.lastMessage.timestamp).getTime());
      }
      const workerObj = this.workers().find(w => w.id === (senderId === 'c1' ? receiverId : senderId));
      const newConv: Conversation = {
        id: 'conv-' + Date.now(),
        otherUser: {
          id: senderId === 'c1' ? receiverId : senderId,
          name: workerObj?.name || 'مستخدم عمال',
          avatar: workerObj?.avatar || 'https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?w=100',
          role: workerObj ? 'worker' : 'client',
          profession: workerObj?.profession
        },
        lastMessage: newMessage,
        unreadCount: senderId === 'c1' ? 0 : 1
      };
      return [newConv, ...convs];
    });
    if (senderId === 'c1') {
      setTimeout(() => { this.simulateWorkerReply(receiverId, senderId); }, 3000);
    }
  }

  private simulateWorkerReply(workerId: string, clientId: string) {
    const replies = ['تمام يا فندم قرأت رسالتك وجاهز للشغل.', 'تمام يا باشا، هبص على طلب الحجز.', 'يا ريت تبعتلي صور للمشكلة.', 'حبيبي يا فندم تسلم، هكون عندك في الميعاد.'];
    const randomReply = replies[Math.floor(Math.random() * replies.length)];
    const newMessage: Message = {
      id: 'msg-' + Date.now(), senderId: workerId, receiverId: clientId,
      content: randomReply, timestamp: new Date().toISOString(), read: false
    };
    this.messages.update(curr => [...curr, newMessage]);
    this.conversations.update(convs =>
      convs.map(c => c.otherUser.id === workerId ? { ...c, lastMessage: newMessage, unreadCount: c.unreadCount + 1 } : c)
    );
  }
}

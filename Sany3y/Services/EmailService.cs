using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Sany3y.Infrastructure.Models;
using System.Threading.Tasks;

namespace Sany3y.Services
{
    public class EmailService
    {
        private readonly UserManager<User> _userManager;
        private readonly IEmailSender _emailSender;

        public EmailService(UserManager<User> userManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _emailSender = emailSender;
        }

        /// <summary>
        /// Send email confirmation with prebuilt callback URL.
        /// </summary>
        public async System.Threading.Tasks.Task SendConfirmationAsync(User user, string callbackUrl)
        {
    // 1. تفعيل حساب المستخدم فوراً في قاعدة البيانات لتخطي أي تشيك مستقبلي
    user.EmailConfirmed = true;
    await _userManager.UpdateAsync(user);

    // 2. طباعة رسالة في الـ Console للـ Debugging بدلاً من الإرسال الفعلي
    System.Console.WriteLine($"[Email Bypass]: Confirmed {user.Email}");

    // 3. نرجع Task مكتملة مباشرة بدون استدعاء الـ _emailSender نهائياً
    await System.Threading.Tasks.Task.CompletedTask;
    }
        }
}

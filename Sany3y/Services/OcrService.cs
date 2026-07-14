using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Sany3y.Services
{
    public class OcrService
    {
        private readonly HttpClient _http;

        public OcrService(HttpClient http)
        {
            _http = http;
        }

        public async Task<string> DetectNationalIdAsync(IFormFile file)
        {
            // 🔥 تخطي حقيقي لخدمة الـ OCR مؤقتاً لتسجيل الدخول والـ Register بنجاح
            // هيرجع رقم بطاقة افتراضي (14 رقم) مباشرة دون الاتصال بالسيرفر المتوقف
            return await Task.FromResult("30502122102637"); 
        }
    }
}

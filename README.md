# ETicaretAPI 🛒

ETicaretAPI, modern e-ticaret platformlarının ihtiyaç duyduğu temel işlevleri (kullanıcı yönetimi, sepet işlemleri, ürün listeleme ve online ödeme) sunan, **.NET 8** üzerinde inşa edilmiş kapsamlı bir RESTful Web API projesidir.

## 🚀 Proje Hakkında

Bu proje, bir e-ticaret sisteminin arka planında (backend) dönen tüm süreçleri güvenli ve ölçeklenebilir bir şekilde yönetmek için tasarlanmıştır. Projede ödeme altyapısı olarak gerçek dünya senaryolarına uygun **İyzico (Iyzipay)** entegrasyonu kullanılmış, hataların merkezi olarak yönetilmesi için özel **Middleware** yazılmış ve kullanıcı yönetimi **ASP.NET Core Identity** ile sağlanmıştır.

## ✨ Öne Çıkan Özellikler

* **Kullanıcı ve Yetki Yönetimi (Identity & JWT):** ASP.NET Core Identity kullanılarak güvenli kullanıcı kaydı, girişi ve JWT (JSON Web Token) tabanlı endpoint yetkilendirmesi.
* **Online Ödeme Entegrasyonu (İyzico):** Kullanıcıların sepetlerindeki ürünleri kredi kartı ile güvenle satın alabilmesi için `Iyzipay` entegrasyonu. (Test ortamı yapılandırması ile).
* **Akıllı Sepet Sistemi:** Her kullanıcıya özel asenkron sepet (Cart & CartItems) yönetimi, miktar güncelleme ve stok kontrolü.
* **Gelişmiş Sipariş Modülü:** Başarılı ödeme sonrası otomatik sipariş oluşturma, satın alınan ürünlerin stoktan düşülmesi ve sepetin temizlenmesi.
* **Merkezi Hata Yönetimi:** `ExceptionHandling` middleware'i sayesinde uygulama genelindeki tüm hataların standart bir JSON formatında (ProblemDetails) yakalanıp loglanması.

## 🛠️ Kullanılan Teknolojiler

* **Framework:** .NET 8.0 Web API
* **Veritabanı & ORM:** Microsoft SQL Server, Entity Framework Core 8
* **Kimlik Doğrulama:** ASP.NET Core Identity, JWT Bearer
* **Ödeme Geçidi:** Iyzipay API
* **API Dokümantasyonu:** Swagger (Swashbuckle)
* **Mimari Yaklaşım:** N-Tier Klasör Yapısı (Controllers, Services, Entities, DTOs)

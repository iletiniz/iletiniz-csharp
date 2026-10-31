# Changelog / Değişiklik Günlüğü

Bu projedeki önemli değişiklikler bu dosyada belgelenir.
All notable changes to this project will be documented in this file.

Format: [Keep a Changelog](https://keepachangelog.com/en/1.1.0/)
Sürümleme / Versioning: [Semantic Versioning](https://semver.org/spec/v2.0.0.html)

## [Unreleased]

### Added
- `client.Partner.ListApiKeysAsync` ve `client.Partner.RevokeApiKeyAsync`: alt hesabın API anahtarlarını listeleme ve iptal etme; `ChildApiKeyInfo` / `ChildApiKeyList` / `ChildApiKeyStatus` tipleri.

## [0.2.0] - 2026-10-31

### Eklenenler / Added
- `SendMessageParams.WhatsappTemplate` (Meta onaylı şablon), `BulkItemInput.WhatsappComponents`, WhatsApp için `Sender` = `phone_number_id`.
- `ScheduledAt` (tekli ve toplu); `SendStatus.Scheduled`, yanıtlarda `ScheduledId` / `ScheduledAt`.
- `client.Scheduled` (List/Retrieve/Cancel), `client.Reminders` (Create/List/Retrieve/Update/Delete), `client.Partner` (profil, alt hesap, anahtar, kota aktarma, WhatsApp bağlantı linki).
- Yanıtlarda `WhatsappTemplate`, `Sandbox`, `DeliveredVia`.
- `tests/Iletiniz.Tests` xUnit test projesi (sahte transport).

### Düzeltmeler / Fixed
- API hata zarfı `{"error": {"code", "message"}}` artık çözülüyor (`IletinizApiException.Code`, `Message`); düz `{"error": "...", "message": "..."}` biçimi de desteklenmeye devam ediyor.
- `Iletiniz.sln` var olmayan test projesine referans verdiği için derlenmiyordu.

### Değişenler / Changed
- Partner hesap/anahtar oluşturma ve kota aktarımı istekleri yeniden denenmez.
- `.gitignore` artık `tests/` klasörünü dışlamıyor.

## [0.1.0] - 2026-05-12

### Eklenenler / Added
- Açık kaynak proje hijyen dosyaları: LICENSE (MIT), CONTRIBUTING, CODE_OF_CONDUCT, SECURITY
- İki dilli dokümantasyon (Türkçe + English)
- İlk açık kaynak yayını / Initial open-source release

-- 1. ADIM: İşlemleri ve Faturaları Temizle (Bağımlı tablolar)
DELETE FROM IsEmirleri_Log;
DELETE FROM IsEmriDetaylari;
DELETE FROM IsEmirleri;
DELETE FROM Randevular;

-- 2. ADIM: Kullanıcıları Temizle
-- DİKKAT: Burada 'İşletme Sahibi' olan hesabını korumak isteyebilirsin. 
-- Eğer HERKESİ (kendin dahil) silip baştan Register olmak istiyorsan aşağıdaki satırı kullan:
DELETE FROM Kullanicilar;

-- 3. ADIM: Otomatik artan (ID) sayaçlarını 1'den başlatacak şekilde sıfırla
-- (Böylece ilk açacağın iş emri 105 değil, tekrar 1 numara olur)
DBCC CHECKIDENT ('IsEmirleri', RESEED, 0);
DBCC CHECKIDENT ('IsEmriDetaylari', RESEED, 0);
DBCC CHECKIDENT ('Randevular', RESEED, 0);
DBCC CHECKIDENT ('Kullanicilar', RESEED, 0);
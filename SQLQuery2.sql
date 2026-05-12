-- Hangi veritabanında çalışacağımızı KESİN olarak belirtiyoruz
USE AracServisDB;
GO

-- 1. Önce Email kolonunu koruyan o "Benzersizlik" kuralını (Constraint) siliyoruz
ALTER TABLE Kullanicilar DROP CONSTRAINT UQ__Kullanic__A9D10534F51D3EBF;
GO

-- 2. Kalkan kalkanın ardından artık Email kolonunu rahatça silebiliriz
ALTER TABLE Kullanicilar DROP COLUMN Email;
GO

-- 3. Yeni sistemimiz olan KullaniciAdi ve OnayKodu kolonlarını ekliyoruz
ALTER TABLE Kullanicilar ADD KullaniciAdi NVARCHAR(20) NOT NULL;
ALTER TABLE Kullanicilar ADD OnayKodu NVARCHAR(10) NULL;
GO

-- 4. Kullanıcı Adı'nın benzersiz (Unique) olmasını sağlıyoruz
ALTER TABLE Kullanicilar ADD CONSTRAINT UQ_KullaniciAdi UNIQUE (KullaniciAdi);
GO
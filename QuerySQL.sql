-- =========================================
-- CREATE TABLE : tb_Approval
-- =========================================
CREATE TABLE tb_Approval (
    id INT IDENTITY(1,1) PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    reason VARCHAR(500) NULL,
    status VARCHAR(50) NOT NULL
);


-- =========================================
-- INSERT DATA ตามตัวอย่างในรูป
-- สถานะ :
-- PENDING = รออนุมัติ
-- APPROVED = อนุมัติ
-- REJECTED = ไม่อนุมัติ
-- =========================================

INSERT INTO tb_Approval (name, reason, status)
VALUES 
('รายการที่ 1', NULL, 'PENDING'),
('รายการที่ 2', NULL, 'APPROVED'),
('รายการที่ 3', NULL, 'REJECTED'),
('รายการที่ 4', NULL, 'PENDING'),
('รายการที่ 5', NULL, 'APPROVED'),
('รายการที่ 6', NULL, 'REJECTED'),
('รายการที่ 7', NULL, 'PENDING'),
('รายการที่ 8', NULL, 'APPROVED'),
('รายการที่ 9', NULL, 'REJECTED'),
('รายการที่ 10', NULL, 'PENDING');
-- =========================================
-- CREATE TABLE : tb_FeedPost
-- =========================================
CREATE TABLE tb_FeedPost (
    id INT IDENTITY(1,1) PRIMARY KEY,
    username NVARCHAR(100) NOT NULL,
    postDate DATETIME NOT NULL DEFAULT GETDATE(),
    imageUrl NVARCHAR(500) NULL
);

-- =========================================
-- INSERT DATA TABLE : tb_FeedPost
-- =========================================
INSERT INTO tb_FeedPost (username, avatar, postDate, imageUrl)
VALUES ('Sangsom', 'S', getdate(), 'https://image.dogilike.com/shareimg/contentimg/2017/saiparn/Dogs%20and%20Cats/MamdH%203.jpg');



-- =========================================
-- CREATE TABLE : tb_FeedComment
-- =========================================
CREATE TABLE tb_FeedComment (
    id INT IDENTITY(1,1) PRIMARY KEY,
    postId INT NOT NULL,
    username NVARCHAR(100) NOT NULL,
    commentText NVARCHAR(MAX) NOT NULL,
    createdDate DATETIME NOT NULL DEFAULT GETDATE()
);
-- =========================================
-- INSERT DATA TABLE : tb_FeedComment
-- =========================================
INSERT INTO tb_FeedComment (postId, username, commentText) VALUES ('1', 'Blend 285', 'have a good day')
SET SQLBLANKLINES ON;

/* =============================================================================
   SCRIPT 02: STORED PROCEDURES (CRUD & BUSINESS LOGIC)
   Project: Library Management System
   DBMS: Oracle Database 23ai / 26ai Free (Docker)
   Schema User: LIBRARY_USER
   ============================================================================= */

/* =============================================================================
   SECTION 1: STUDENT STORED PROCEDURES
   ============================================================================= */

-- 1.1 Insert Student (Returns generated StuID)
CREATE OR REPLACE PROCEDURE SP_STUDENT_INSERT
(
    p_stu_name  IN  VARCHAR2,
    p_gender    IN  VARCHAR2,
    p_dob       IN  DATE,
    p_pob       IN  VARCHAR2,
    p_address   IN  VARCHAR2,
    p_phone     IN  VARCHAR2,
    p_email     IN  VARCHAR2,
    p_photo     IN  BLOB,
    p_new_id    OUT NUMBER
)
AS
BEGIN
    INSERT INTO Student
    (
        StuName, Gender, DOB, POB, Address, Phone, Email, Photo
    )
    VALUES
    (
        p_stu_name, p_gender, p_dob, p_pob, p_address, p_phone, p_email, p_photo
    )
    RETURNING StuID INTO p_new_id;
END;
/

-- 1.2 Select All Students
CREATE OR REPLACE PROCEDURE SP_STUDENT_SELECT_ALL
(
    p_cursor OUT SYS_REFCURSOR
)
AS
BEGIN
    OPEN p_cursor FOR
        SELECT StuID, StuName, Gender, DOB, POB, Address, Phone, Email, Photo
        FROM Student
        ORDER BY StuID DESC;
END;
/

-- 1.3 Select Student By ID
CREATE OR REPLACE PROCEDURE SP_STUDENT_SELECT_BY_ID
(
    p_stu_id IN  NUMBER,
    p_cursor OUT SYS_REFCURSOR
)
AS
BEGIN
    OPEN p_cursor FOR
        SELECT StuID, StuName, Gender, DOB, POB, Address, Phone, Email, Photo
        FROM Student
        WHERE StuID = p_stu_id;
END;
/

-- 1.4 Update Student
CREATE OR REPLACE PROCEDURE SP_STUDENT_UPDATE
(
    p_stu_id    IN NUMBER,
    p_stu_name  IN VARCHAR2,
    p_gender    IN VARCHAR2,
    p_dob       IN DATE,
    p_pob       IN VARCHAR2,
    p_address   IN VARCHAR2,
    p_phone     IN VARCHAR2,
    p_email     IN VARCHAR2,
    p_photo     IN BLOB
)
AS
BEGIN
    UPDATE Student
    SET StuName  = p_stu_name,
        Gender   = p_gender,
        DOB      = p_dob,
        POB      = p_pob,
        Address  = p_address,
        Phone    = p_phone,
        Email    = p_email,
        Photo    = p_photo
    WHERE StuID  = p_stu_id;
END;
/

-- 1.5 Delete Student
CREATE OR REPLACE PROCEDURE SP_STUDENT_DELETE
(
    p_stu_id IN NUMBER
)
AS
BEGIN
    DELETE FROM Student
    WHERE StuID = p_stu_id;
END;
/

-- 1.6 Search Students
CREATE OR REPLACE PROCEDURE SP_STUDENT_SEARCH
(
    p_keyword IN  VARCHAR2,
    p_cursor  OUT SYS_REFCURSOR
)
AS
    v_kw VARCHAR2(150) := '%' || LOWER(TRIM(p_keyword)) || '%';
BEGIN
    OPEN p_cursor FOR
        SELECT StuID, StuName, Gender, DOB, POB, Address, Phone, Email, Photo
        FROM Student
        WHERE LOWER(StuName) LIKE v_kw
           OR LOWER(Phone) LIKE v_kw
           OR LOWER(Email) LIKE v_kw
           OR LOWER(Address) LIKE v_kw
           OR TO_CHAR(StuID) LIKE v_kw
        ORDER BY StuID DESC;
END;
/


/* =============================================================================
   SECTION 2: AUTHOR STORED PROCEDURES
   ============================================================================= */

-- 2.1 Insert Author (Returns generated AuthorID)
CREATE OR REPLACE PROCEDURE SP_AUTHOR_INSERT
(
    p_author_name IN  VARCHAR2,
    p_gender      IN  VARCHAR2,
    p_dob         IN  DATE,
    p_pob         IN  VARCHAR2,
    p_address     IN  VARCHAR2,
    p_phone       IN  VARCHAR2,
    p_email       IN  VARCHAR2,
    p_photo       IN  BLOB,
    p_new_id      OUT NUMBER
)
AS
BEGIN
    INSERT INTO Author
    (
        AuthorName, Gender, DOB, POB, Address, Phone, Email, Photo
    )
    VALUES
    (
        p_author_name, p_gender, p_dob, p_pob, p_address, p_phone, p_email, p_photo
    )
    RETURNING AuthorID INTO p_new_id;
END;
/

-- 2.2 Select All Authors
CREATE OR REPLACE PROCEDURE SP_AUTHOR_SELECT_ALL
(
    p_cursor OUT SYS_REFCURSOR
)
AS
BEGIN
    OPEN p_cursor FOR
        SELECT AuthorID, AuthorName, Gender, DOB, POB, Address, Phone, Email, Photo
        FROM Author
        ORDER BY AuthorID DESC;
END;
/

-- 2.3 Select Author By ID
CREATE OR REPLACE PROCEDURE SP_AUTHOR_SELECT_BY_ID
(
    p_author_id IN  NUMBER,
    p_cursor    OUT SYS_REFCURSOR
)
AS
BEGIN
    OPEN p_cursor FOR
        SELECT AuthorID, AuthorName, Gender, DOB, POB, Address, Phone, Email, Photo
        FROM Author
        WHERE AuthorID = p_author_id;
END;
/

-- 2.4 Update Author
CREATE OR REPLACE PROCEDURE SP_AUTHOR_UPDATE
(
    p_author_id   IN NUMBER,
    p_author_name IN VARCHAR2,
    p_gender      IN VARCHAR2,
    p_dob         IN DATE,
    p_pob         IN VARCHAR2,
    p_address     IN VARCHAR2,
    p_phone       IN VARCHAR2,
    p_email       IN VARCHAR2,
    p_photo       IN BLOB
)
AS
BEGIN
    UPDATE Author
    SET AuthorName = p_author_name,
        Gender     = p_gender,
        DOB        = p_dob,
        POB        = p_pob,
        Address    = p_address,
        Phone      = p_phone,
        Email      = p_email,
        Photo      = p_photo
    WHERE AuthorID = p_author_id;
END;
/

-- 2.5 Delete Author
CREATE OR REPLACE PROCEDURE SP_AUTHOR_DELETE
(
    p_author_id IN NUMBER
)
AS
BEGIN
    DELETE FROM Author
    WHERE AuthorID = p_author_id;
END;
/

-- 2.6 Search Authors
CREATE OR REPLACE PROCEDURE SP_AUTHOR_SEARCH
(
    p_keyword IN  VARCHAR2,
    p_cursor  OUT SYS_REFCURSOR
)
AS
    v_kw VARCHAR2(150) := '%' || LOWER(TRIM(p_keyword)) || '%';
BEGIN
    OPEN p_cursor FOR
        SELECT AuthorID, AuthorName, Gender, DOB, POB, Address, Phone, Email, Photo
        FROM Author
        WHERE LOWER(AuthorName) LIKE v_kw
           OR LOWER(Phone) LIKE v_kw
           OR LOWER(Email) LIKE v_kw
           OR LOWER(Address) LIKE v_kw
           OR TO_CHAR(AuthorID) LIKE v_kw
        ORDER BY AuthorID DESC;
END;
/


/* =============================================================================
   SECTION 3: BOOK STORED PROCEDURES (JOIN WITH BOOKTYPE)
   ============================================================================= */

-- 3.1 Insert Book (Returns generated BookID)
CREATE OR REPLACE PROCEDURE SP_BOOK_INSERT
(
    p_book_title    IN  VARCHAR2,
    p_book_type_id  IN  NUMBER,
    p_publish_date  IN  DATE,
    p_num_of_pages  IN  NUMBER,
    p_num_copies    IN  NUMBER,
    p_edition       IN  VARCHAR2,
    p_publisher     IN  VARCHAR2,
    p_book_source   IN  VARCHAR2,
    p_remark        IN  VARCHAR2,
    p_new_id        OUT NUMBER
)
AS
BEGIN
    INSERT INTO Book
    (
        BookTitle, BookTypeID, PublishDate, NumOfPages,
        NumCopies, Edition, Publisher, BookSource, Remark
    )
    VALUES
    (
        p_book_title, p_book_type_id, p_publish_date, p_num_of_pages,
        p_num_copies, p_edition, p_publisher, p_book_source, p_remark
    )
    RETURNING BookID INTO p_new_id;
END;
/

-- 3.2 Select All Books (Includes BookTypeName from BookType join)
CREATE OR REPLACE PROCEDURE SP_BOOK_SELECT_ALL
(
    p_cursor OUT SYS_REFCURSOR
)
AS
BEGIN
    OPEN p_cursor FOR
        SELECT 
            b.BookID,
            b.BookTitle,
            b.BookTypeID,
            bt.BookTypeName,
            b.PublishDate,
            b.NumOfPages,
            b.NumCopies,
            b.Edition,
            b.Publisher,
            b.BookSource,
            b.Remark
        FROM Book b
        JOIN BookType bt ON b.BookTypeID = bt.BookTypeID
        ORDER BY b.BookID DESC;
END;
/

-- 3.3 Select Book By ID
CREATE OR REPLACE PROCEDURE SP_BOOK_SELECT_BY_ID
(
    p_book_id IN  NUMBER,
    p_cursor  OUT SYS_REFCURSOR
)
AS
BEGIN
    OPEN p_cursor FOR
        SELECT 
            b.BookID,
            b.BookTitle,
            b.BookTypeID,
            bt.BookTypeName,
            b.PublishDate,
            b.NumOfPages,
            b.NumCopies,
            b.Edition,
            b.Publisher,
            b.BookSource,
            b.Remark
        FROM Book b
        JOIN BookType bt ON b.BookTypeID = bt.BookTypeID
        WHERE b.BookID = p_book_id;
END;
/

-- 3.4 Update Book
CREATE OR REPLACE PROCEDURE SP_BOOK_UPDATE
(
    p_book_id       IN NUMBER,
    p_book_title    IN VARCHAR2,
    p_book_type_id  IN NUMBER,
    p_publish_date  IN DATE,
    p_num_of_pages  IN NUMBER,
    p_num_copies    IN NUMBER,
    p_edition       IN VARCHAR2,
    p_publisher     IN VARCHAR2,
    p_book_source   IN VARCHAR2,
    p_remark        IN VARCHAR2
)
AS
BEGIN
    UPDATE Book
    SET BookTitle   = p_book_title,
        BookTypeID  = p_book_type_id,
        PublishDate = p_publish_date,
        NumOfPages  = p_num_of_pages,
        NumCopies   = p_num_copies,
        Edition     = p_edition,
        Publisher   = p_publisher,
        BookSource  = p_book_source,
        Remark      = p_remark
    WHERE BookID    = p_book_id;
END;
/

-- 3.5 Delete Book
CREATE OR REPLACE PROCEDURE SP_BOOK_DELETE
(
    p_book_id IN NUMBER
)
AS
BEGIN
    DELETE FROM Book
    WHERE BookID = p_book_id;
END;
/

-- 3.6 Search Books
CREATE OR REPLACE PROCEDURE SP_BOOK_SEARCH
(
    p_keyword IN  VARCHAR2,
    p_cursor  OUT SYS_REFCURSOR
)
AS
    v_kw VARCHAR2(150) := '%' || LOWER(TRIM(p_keyword)) || '%';
BEGIN
    OPEN p_cursor FOR
        SELECT 
            b.BookID,
            b.BookTitle,
            b.BookTypeID,
            bt.BookTypeName,
            b.PublishDate,
            b.NumOfPages,
            b.NumCopies,
            b.Edition,
            b.Publisher,
            b.BookSource,
            b.Remark
        FROM Book b
        JOIN BookType bt ON b.BookTypeID = bt.BookTypeID
        WHERE LOWER(b.BookTitle) LIKE v_kw
           OR LOWER(bt.BookTypeName) LIKE v_kw
           OR LOWER(b.Publisher) LIKE v_kw
           OR LOWER(b.Edition) LIKE v_kw
           OR TO_CHAR(b.BookID) LIKE v_kw
        ORDER BY b.BookID DESC;
END;
/


/* =============================================================================
   SECTION 4: OTHER USEFUL STORED PROCEDURES
   ============================================================================= */

-- 4.1 Select All BookTypes (For dropdowns/ComboBoxes)
CREATE OR REPLACE PROCEDURE SP_BOOKTYPE_SELECT_ALL
(
    p_cursor OUT SYS_REFCURSOR
)
AS
BEGIN
    OPEN p_cursor FOR
        SELECT BookTypeID, BookTypeName
        FROM BookType
        ORDER BY BookTypeName ASC;
END;
/

-- 4.2 Select All Librarians (For staff dropdowns)
CREATE OR REPLACE PROCEDURE SP_LIBRARIAN_SELECT_ALL
(
    p_cursor OUT SYS_REFCURSOR
)
AS
BEGIN
    OPEN p_cursor FOR
        SELECT LibID, LibName, Gender, DOB, POB, Address, Phone, Email
        FROM Librarian
        ORDER BY LibName ASC;
END;
/

-- 4.3 Borrow Header Insert
CREATE OR REPLACE PROCEDURE SP_BORROW_INSERT
(
    p_stu_id      IN  NUMBER,
    p_lib_id      IN  NUMBER,
    p_borrow_date IN  DATE,
    p_remark      IN  VARCHAR2,
    p_new_id      OUT NUMBER
)
AS
BEGIN
    INSERT INTO Borrow
    (
        StuID, LibID, BorrowDate, Remark
    )
    VALUES
    (
        p_stu_id, p_lib_id, NVL(p_borrow_date, SYSDATE), p_remark
    )
    RETURNING BorrowID INTO p_new_id;
END;
/

-- 4.4 Borrow Line Item Insert (Stock verification & deduction is enforced by triggers)
CREATE OR REPLACE PROCEDURE SP_BORROWBOOK_INSERT
(
    p_borrow_id  IN NUMBER,
    p_book_id    IN NUMBER,
    p_qty_borrow IN NUMBER
)
AS
BEGIN
    INSERT INTO BorrowBook
    (
        BorrowID, BookID, QtyBorrow, IsReturned, ReturnDate
    )
    VALUES
    (
        p_borrow_id, p_book_id, p_qty_borrow, 0, NULL
    );
END;
/

-- 4.5 Return Book Transaction
-- Validates remaining borrowed quantity, records the return, updates BorrowBook return status,
-- and relies on TRG_RETURNBOOK_INCREASE_STOCK to restore book inventory safely.
CREATE OR REPLACE PROCEDURE SP_RETURN_BOOK
(
    p_borrow_id     IN  NUMBER,
    p_book_id       IN  NUMBER,
    p_lib_id        IN  NUMBER,
    p_qty_return    IN  NUMBER,
    p_remark        IN  VARCHAR2,
    p_new_return_id OUT NUMBER
)
AS
    v_qty_borrowed       NUMBER;
    v_already_returned   NUMBER := 0;
    v_remaining_borrowed NUMBER;
BEGIN
    -- 1. Verify that the borrow record exists
    BEGIN
        SELECT QtyBorrow INTO v_qty_borrowed
        FROM BorrowBook
        WHERE BorrowID = p_borrow_id AND BookID = p_book_id;
    EXCEPTION
        WHEN NO_DATA_FOUND THEN
            RAISE_APPLICATION_ERROR(-20004, 'No matching borrow line found for Borrow ID ' || p_borrow_id || ' and Book ID ' || p_book_id);
    END;

    -- 2. Calculate quantity already returned
    SELECT NVL(SUM(QtyReturn), 0)
    INTO v_already_returned
    FROM BOOK_RETURN
    WHERE BorrowID = p_borrow_id AND BookID = p_book_id;

    v_remaining_borrowed := v_qty_borrowed - v_already_returned;

    -- 3. Validate that return quantity does not exceed remaining borrowed
    IF p_qty_return > v_remaining_borrowed THEN
        RAISE_APPLICATION_ERROR(
            -20003, 
            'Cannot return ' || p_qty_return || ' copies. Only ' || v_remaining_borrowed || ' borrowed copies remain outstanding.'
        );
    END IF;

    -- 4. Insert into BOOK_RETURN (Triggers TRG_RETURNBOOK_INCREASE_STOCK to increase Book.NumCopies)
    INSERT INTO BOOK_RETURN
    (
        BorrowID, BookID, LibID, QtyReturn, ReturnDate, Remark
    )
    VALUES
    (
        p_borrow_id, p_book_id, p_lib_id, p_qty_return, SYSDATE, p_remark
    )
    RETURNING ReturnID INTO p_new_return_id;

    -- 5. If all borrowed copies are now returned, mark BorrowBook.IsReturned = 1
    IF (v_already_returned + p_qty_return) >= v_qty_borrowed THEN
        UPDATE BorrowBook
        SET IsReturned = 1,
            ReturnDate = SYSDATE
        WHERE BorrowID = p_borrow_id AND BookID = p_book_id;
    ELSE
        UPDATE BorrowBook
        SET ReturnDate = SYSDATE
        WHERE BorrowID = p_borrow_id AND BookID = p_book_id;
    END IF;
END;
/

-- 4.6 User Login
-- Note: Plaintext comparison used for university demo simplicity.
-- In production systems, secure password hashing (such as Argon2/BCrypt/SHA-256 with salt) is mandatory.
CREATE OR REPLACE PROCEDURE SP_LOGIN
(
    p_username IN  VARCHAR2,
    p_password IN  VARCHAR2,
    p_cursor   OUT SYS_REFCURSOR
)
AS
BEGIN
    OPEN p_cursor FOR
        SELECT 
            u.LibID,
            u.UserName,
            u.UserType,
            l.LibName,
            l.Email
        FROM APP_USER u
        JOIN Librarian l ON u.LibID = l.LibID
        WHERE LOWER(u.UserName) = LOWER(TRIM(p_username))
          AND u.UserPassword    = p_password;
END;
/

SET DEFINE OFF;
SET SQLBLANKLINES ON;

/* =============================================================================
   SCRIPT 06: SAMPLE DATA POPULATION
   Project: Library Management System
   DBMS: Oracle Database 23ai / 26ai Free (Docker)
   Schema User: LIBRARY_USER
   ============================================================================= */

-- Clear existing data if any (in correct child-to-parent order)
DELETE FROM BOOK_RETURN;
DELETE FROM BorrowBook;
DELETE FROM Borrow;
DELETE FROM BookAuthor;
DELETE FROM Book;
DELETE FROM APP_USER;
DELETE FROM Librarian;
DELETE FROM Student;
DELETE FROM Author;
DELETE FROM BookType;
COMMIT;

/* -----------------------------------------------------------------------------
   1. BOOK TYPES (Categories)
   ----------------------------------------------------------------------------- */
INSERT INTO BookType (BookTypeName) VALUES ('Computer Science');
INSERT INTO BookType (BookTypeName) VALUES ('Software Engineering');
INSERT INTO BookType (BookTypeName) VALUES ('Data Science & AI');
INSERT INTO BookType (BookTypeName) VALUES ('Literature & Fiction');
INSERT INTO BookType (BookTypeName) VALUES ('Business & Economics');
INSERT INTO BookType (BookTypeName) VALUES ('History & Philosophy');
COMMIT;

/* -----------------------------------------------------------------------------
   2. AUTHORS
   ----------------------------------------------------------------------------- */
INSERT INTO Author (AuthorName, Gender, DOB, POB, Address, Phone, Email, Photo)
VALUES ('Robert C. Martin', 'Male', TO_DATE('1952-12-05', 'YYYY-MM-DD'), 'Chicago, USA', 'Illinois, USA', '+1-555-0101', 'unclebob@cleancoder.com', NULL);

INSERT INTO Author (AuthorName, Gender, DOB, POB, Address, Phone, Email, Photo)
VALUES ('Martin Fowler', 'Male', TO_DATE('1963-12-18', 'YYYY-MM-DD'), 'Walsall, UK', 'Melrose, MA, USA', '+1-555-0102', 'fowler@thoughtworks.com', NULL);

INSERT INTO Author (AuthorName, Gender, DOB, POB, Address, Phone, Email, Photo)
VALUES ('Joshua Bloch', 'Male', TO_DATE('1961-08-28', 'YYYY-MM-DD'), 'Long Island, USA', 'San Jose, CA, USA', '+1-555-0103', 'jbloch@effectivejava.com', NULL);

INSERT INTO Author (AuthorName, Gender, DOB, POB, Address, Phone, Email, Photo)
VALUES ('Donald E. Knuth', 'Male', TO_DATE('1938-01-10', 'YYYY-MM-DD'), 'Milwaukee, USA', 'Stanford, CA, USA', '+1-555-0104', 'knuth@cs.stanford.edu', NULL);

INSERT INTO Author (AuthorName, Gender, DOB, POB, Address, Phone, Email, Photo)
VALUES ('Eric Evans', 'Male', TO_DATE('1965-03-15', 'YYYY-MM-DD'), 'Portland, USA', 'Boston, MA, USA', '+1-555-0105', 'eevans@domainlanguage.com', NULL);

INSERT INTO Author (AuthorName, Gender, DOB, POB, Address, Phone, Email, Photo)
VALUES ('Andrew S. Tanenbaum', 'Male', TO_DATE('1944-03-16', 'YYYY-MM-DD'), 'New York, USA', 'Amsterdam, Netherlands', '+31-20-555-0106', 'ast@cs.vu.nl', NULL);
COMMIT;

/* -----------------------------------------------------------------------------
   3. STUDENTS (Members)
   ----------------------------------------------------------------------------- */
INSERT INTO Student (StuName, Gender, DOB, POB, Address, Phone, Email, Photo)
VALUES ('Sophea Tan', 'Male', TO_DATE('2002-05-14', 'YYYY-MM-DD'), 'Phnom Penh', 'St. 271, Phnom Penh', '012-345-678', 'sophea.tan@student.edu', NULL);

INSERT INTO Student (StuName, Gender, DOB, POB, Address, Phone, Email, Photo)
VALUES ('Vannak Sok', 'Male', TO_DATE('2001-11-20', 'YYYY-MM-DD'), 'Battambang', 'Toul Kork, Phnom Penh', '098-765-432', 'vannak.sok@student.edu', NULL);

INSERT INTO Student (StuName, Gender, DOB, POB, Address, Phone, Email, Photo)
VALUES ('Dara Chea', 'Male', TO_DATE('2003-02-10', 'YYYY-MM-DD'), 'Siem Reap', 'BKK1, Phnom Penh', '077-112-233', 'dara.chea@student.edu', NULL);

INSERT INTO Student (StuName, Gender, DOB, POB, Address, Phone, Email, Photo)
VALUES ('Bopha Chan', 'Female', TO_DATE('2002-08-25', 'YYYY-MM-DD'), 'Kampong Cham', 'Sensok, Phnom Penh', '015-889-900', 'bopha.chan@student.edu', NULL);

INSERT INTO Student (StuName, Gender, DOB, POB, Address, Phone, Email, Photo)
VALUES ('Sreymom Pich', 'Female', TO_DATE('2003-07-19', 'YYYY-MM-DD'), 'Kandal', 'Russey Keo, Phnom Penh', '089-445-566', 'sreymom.pich@student.edu', NULL);
COMMIT;

/* -----------------------------------------------------------------------------
   4. LIBRARIANS & APPLICATION USERS (1-to-1 Relationship)
   ----------------------------------------------------------------------------- */
DECLARE
    v_lib1_id NUMBER;
    v_lib2_id NUMBER;
BEGIN
    -- Librarian 1 (Administrator)
    INSERT INTO Librarian (LibName, Gender, DOB, POB, Address, Phone, Email, Photo)
    VALUES ('System Administrator', 'Male', TO_DATE('1990-01-01', 'YYYY-MM-DD'), 'Phnom Penh', 'Head Office, Library Main Building', '023-888-999', 'admin@library.edu', NULL)
    RETURNING LibID INTO v_lib1_id;

    -- Admin User Account (admin / 123 / Admin)
    INSERT INTO APP_USER (LibID, UserName, UserPassword, UserType)
    VALUES (v_lib1_id, 'admin', '123', 'Admin');

    -- Librarian 2 (Staff)
    INSERT INTO Librarian (LibName, Gender, DOB, POB, Address, Phone, Email, Photo)
    VALUES ('Sarah Connor', 'Female', TO_DATE('1995-06-12', 'YYYY-MM-DD'), 'Siem Reap', 'Library Circulation Desk #2', '012-777-666', 'sarah.connor@library.edu', NULL)
    RETURNING LibID INTO v_lib2_id;

    -- Staff User Account
    INSERT INTO APP_USER (LibID, UserName, UserPassword, UserType)
    VALUES (v_lib2_id, 'sarah', '123', 'Librarian');

    COMMIT;
END;
/

/* -----------------------------------------------------------------------------
   5. BOOKS (Catalog)
   ----------------------------------------------------------------------------- */
DECLARE
    v_cs_id  NUMBER;
    v_se_id  NUMBER;
    v_ai_id  NUMBER;
    v_lit_id NUMBER;
BEGIN
    SELECT BookTypeID INTO v_cs_id  FROM BookType WHERE BookTypeName = 'Computer Science';
    SELECT BookTypeID INTO v_se_id  FROM BookType WHERE BookTypeName = 'Software Engineering';
    SELECT BookTypeID INTO v_ai_id  FROM BookType WHERE BookTypeName = 'Data Science & AI';
    SELECT BookTypeID INTO v_lit_id FROM BookType WHERE BookTypeName = 'Literature & Fiction';

    -- Book 1
    INSERT INTO Book (BookTitle, BookTypeID, PublishDate, NumOfPages, NumCopies, Edition, Publisher, BookSource, Remark)
    VALUES ('Clean Code: A Handbook of Agile Software Craftsmanship', v_se_id, TO_DATE('2008-08-01', 'YYYY-MM-DD'), 464, 10, '1st Edition', 'Prentice Hall', 'Direct Purchase', 'Fundamental reading for clean architecture');

    -- Book 2
    INSERT INTO Book (BookTitle, BookTypeID, PublishDate, NumOfPages, NumCopies, Edition, Publisher, BookSource, Remark)
    VALUES ('Refactoring: Improving the Design of Existing Code', v_se_id, TO_DATE('2018-11-20', 'YYYY-MM-DD'), 448, 8, '2nd Edition', 'Addison-Wesley', 'Direct Purchase', 'Code smells and restructuring');

    -- Book 3
    INSERT INTO Book (BookTitle, BookTypeID, PublishDate, NumOfPages, NumCopies, Edition, Publisher, BookSource, Remark)
    VALUES ('Effective Java', v_se_id, TO_DATE('2017-12-27', 'YYYY-MM-DD'), 412, 12, '3rd Edition', 'Addison-Wesley', 'University Grant', 'Best practices for modern Java programming');

    -- Book 4
    INSERT INTO Book (BookTitle, BookTypeID, PublishDate, NumOfPages, NumCopies, Edition, Publisher, BookSource, Remark)
    VALUES ('The Art of Computer Programming, Vol 1: Fundamental Algorithms', v_cs_id, TO_DATE('1997-07-17', 'YYYY-MM-DD'), 672, 5, '3rd Edition', 'Addison-Wesley', 'Special Donation', 'Definitive reference for computer science');

    -- Book 5
    INSERT INTO Book (BookTitle, BookTypeID, PublishDate, NumOfPages, NumCopies, Edition, Publisher, BookSource, Remark)
    VALUES ('Domain-Driven Design: Tackling Complexity in the Heart of Software', v_se_id, TO_DATE('2003-08-30', 'YYYY-MM-DD'), 560, 6, '1st Edition', 'Addison-Wesley', 'Direct Purchase', 'Strategic enterprise domain modeling');

    -- Book 6
    INSERT INTO Book (BookTitle, BookTypeID, PublishDate, NumOfPages, NumCopies, Edition, Publisher, BookSource, Remark)
    VALUES ('Computer Networks', v_cs_id, TO_DATE('2021-01-01', 'YYYY-MM-DD'), 944, 15, '6th Edition', 'Pearson', 'Bookstore Procurement', 'Standard university textbook on computer networking');

    -- Book 7
    INSERT INTO Book (BookTitle, BookTypeID, PublishDate, NumOfPages, NumCopies, Edition, Publisher, BookSource, Remark)
    VALUES ('Modern Operating Systems', v_cs_id, TO_DATE('2014-03-20', 'YYYY-MM-DD'), 1136, 12, '4th Edition', 'Pearson', 'Bookstore Procurement', 'Comprehensive coverage of OS fundamentals');

    -- Book 8
    INSERT INTO Book (BookTitle, BookTypeID, PublishDate, NumOfPages, NumCopies, Edition, Publisher, BookSource, Remark)
    VALUES ('Artificial Intelligence: A Modern Approach', v_ai_id, TO_DATE('2020-04-28', 'YYYY-MM-DD'), 1152, 9, '4th Edition', 'Pearson', 'Academic Grant', 'The leading textbook in artificial intelligence');

    -- Book 9
    INSERT INTO Book (BookTitle, BookTypeID, PublishDate, NumOfPages, NumCopies, Edition, Publisher, BookSource, Remark)
    VALUES ('Design Patterns: Elements of Reusable Object-Oriented Software', v_se_id, TO_DATE('1994-10-31', 'YYYY-MM-DD'), 416, 7, '1st Edition', 'Addison-Wesley', 'Donation', 'Gang of Four architectural design patterns');

    -- Book 10
    INSERT INTO Book (BookTitle, BookTypeID, PublishDate, NumOfPages, NumCopies, Edition, Publisher, BookSource, Remark)
    VALUES ('Database System Concepts', v_cs_id, TO_DATE('2019-02-25', 'YYYY-MM-DD'), 1376, 14, '7th Edition', 'McGraw-Hill', 'Library Procurement', 'Core text on database engines, indexing, and SQL');

    COMMIT;
END;
/

/* -----------------------------------------------------------------------------
   6. BOOKAUTHOR ASSOCIATIONS (Many-to-Many)
   ----------------------------------------------------------------------------- */
DECLARE
    v_b1 NUMBER; v_b2 NUMBER; v_b3 NUMBER; v_b4 NUMBER; v_b5 NUMBER; v_b6 NUMBER; v_b7 NUMBER;
    v_a1 NUMBER; v_a2 NUMBER; v_a3 NUMBER; v_a4 NUMBER; v_a5 NUMBER; v_a6 NUMBER;
BEGIN
    SELECT BookID INTO v_b1 FROM Book WHERE BookTitle LIKE 'Clean Code%';
    SELECT BookID INTO v_b2 FROM Book WHERE BookTitle LIKE 'Refactoring%';
    SELECT BookID INTO v_b3 FROM Book WHERE BookTitle LIKE 'Effective Java%';
    SELECT BookID INTO v_b4 FROM Book WHERE BookTitle LIKE 'The Art of Computer Programming%';
    SELECT BookID INTO v_b5 FROM Book WHERE BookTitle LIKE 'Domain-Driven Design%';
    SELECT BookID INTO v_b6 FROM Book WHERE BookTitle LIKE 'Computer Networks%';
    SELECT BookID INTO v_b7 FROM Book WHERE BookTitle LIKE 'Modern Operating Systems%';

    SELECT AuthorID INTO v_a1 FROM Author WHERE AuthorName = 'Robert C. Martin';
    SELECT AuthorID INTO v_a2 FROM Author WHERE AuthorName = 'Martin Fowler';
    SELECT AuthorID INTO v_a3 FROM Author WHERE AuthorName = 'Joshua Bloch';
    SELECT AuthorID INTO v_a4 FROM Author WHERE AuthorName = 'Donald E. Knuth';
    SELECT AuthorID INTO v_a5 FROM Author WHERE AuthorName = 'Eric Evans';
    SELECT AuthorID INTO v_a6 FROM Author WHERE AuthorName = 'Andrew S. Tanenbaum';

    INSERT INTO BookAuthor (BookID, AuthorID, AuthorDate, Remark) VALUES (v_b1, v_a1, TO_DATE('2008-08-01', 'YYYY-MM-DD'), 'Sole author');
    INSERT INTO BookAuthor (BookID, AuthorID, AuthorDate, Remark) VALUES (v_b2, v_a2, TO_DATE('2018-11-20', 'YYYY-MM-DD'), 'Lead author');
    INSERT INTO BookAuthor (BookID, AuthorID, AuthorDate, Remark) VALUES (v_b3, v_a3, TO_DATE('2017-12-27', 'YYYY-MM-DD'), 'Lead author');
    INSERT INTO BookAuthor (BookID, AuthorID, AuthorDate, Remark) VALUES (v_b4, v_a4, TO_DATE('1997-07-17', 'YYYY-MM-DD'), 'Seminal work');
    INSERT INTO BookAuthor (BookID, AuthorID, AuthorDate, Remark) VALUES (v_b5, v_a5, TO_DATE('2003-08-30', 'YYYY-MM-DD'), 'Blue Book creator');
    INSERT INTO BookAuthor (BookID, AuthorID, AuthorDate, Remark) VALUES (v_b6, v_a6, TO_DATE('2021-01-01', 'YYYY-MM-DD'), 'Co-authored networking guide');
    INSERT INTO BookAuthor (BookID, AuthorID, AuthorDate, Remark) VALUES (v_b7, v_a6, TO_DATE('2014-03-20', 'YYYY-MM-DD'), 'OS architecture authority');

    COMMIT;
END;
/

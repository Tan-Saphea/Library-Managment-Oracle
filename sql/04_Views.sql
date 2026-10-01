SET SQLBLANKLINES ON;

/* =============================================================================
   SCRIPT 04: DATABASE VIEWS
   Project: Library Management System
   DBMS: Oracle Database 23ai / 26ai Free (Docker)
   Schema User: LIBRARY_USER
   ============================================================================= */

/* -----------------------------------------------------------------------------
   1. VW_BOOK_DETAILS
   Comprehensive view of books including genre category names.
   ----------------------------------------------------------------------------- */
CREATE OR REPLACE VIEW VW_BOOK_DETAILS AS
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
JOIN BookType bt ON b.BookTypeID = bt.BookTypeID;

/* -----------------------------------------------------------------------------
   2. VW_BOOK_AUTHOR
   Many-to-many relationship view connecting Books with Authors.
   ----------------------------------------------------------------------------- */
CREATE OR REPLACE VIEW VW_BOOK_AUTHOR AS
SELECT 
    ba.BookID,
    b.BookTitle,
    ba.AuthorID,
    a.AuthorName,
    ba.AuthorDate,
    ba.Remark AS AuthorRemark
FROM BookAuthor ba
JOIN Book b   ON ba.BookID = b.BookID
JOIN Author a ON ba.AuthorID = a.AuthorID;

/* -----------------------------------------------------------------------------
   3. VW_BORROW_DETAILS
   Complete view of borrow transactions with student, staff, and line items.
   ----------------------------------------------------------------------------- */
CREATE OR REPLACE VIEW VW_BORROW_DETAILS AS
SELECT 
    br.BorrowID,
    br.StuID,
    s.StuName,
    br.LibID,
    l.LibName,
    br.BorrowDate,
    bb.BookID,
    b.BookTitle,
    bb.QtyBorrow,
    bb.IsReturned,
    bb.ReturnDate,
    br.Remark
FROM Borrow br
JOIN Student s       ON br.StuID = s.StuID
JOIN Librarian l     ON br.LibID = l.LibID
JOIN BorrowBook bb   ON br.BorrowID = bb.BorrowID
JOIN Book b          ON bb.BookID = b.BookID;

/* -----------------------------------------------------------------------------
   4. VW_RETURN_DETAILS
   View of return transactions matching the original assignment diagram.
   ----------------------------------------------------------------------------- */
CREATE OR REPLACE VIEW VW_RETURN_DETAILS AS
SELECT 
    r.ReturnID,
    r.BorrowID,
    s.StuName     AS Student,
    b.BookTitle   AS Book,
    l.LibName     AS Librarian,
    r.QtyReturn,
    r.ReturnDate,
    r.Remark
FROM BOOK_RETURN r
JOIN Borrow br    ON r.BorrowID = br.BorrowID
JOIN Student s    ON br.StuID = s.StuID
JOIN Book b       ON r.BookID = b.BookID
JOIN Librarian l  ON r.LibID = l.LibID;

/* -----------------------------------------------------------------------------
   5. VW_STUDENT_BORROW_HISTORY
   Student-centric history of all active and past borrowings.
   ----------------------------------------------------------------------------- */
CREATE OR REPLACE VIEW VW_STUDENT_BORROW_HISTORY AS
SELECT 
    s.StuID,
    s.StuName,
    s.Gender,
    s.Phone,
    s.Email,
    br.BorrowID,
    br.BorrowDate,
    bb.BookID,
    b.BookTitle,
    bb.QtyBorrow,
    bb.IsReturned,
    bb.ReturnDate,
    l.LibName AS LibrarianName
FROM Student s
JOIN Borrow br      ON s.StuID = br.StuID
JOIN Librarian l    ON br.LibID = l.LibID
JOIN BorrowBook bb  ON br.BorrowID = bb.BorrowID
JOIN Book b         ON bb.BookID = b.BookID;

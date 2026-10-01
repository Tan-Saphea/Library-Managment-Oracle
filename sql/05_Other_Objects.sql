SET SQLBLANKLINES ON;

/* =============================================================================
   SCRIPT 05: COMMENTS, METADATA & VALIDATION UTILITIES
   Project: Library Management System
   DBMS: Oracle Database 23ai / 26ai Free (Docker)
   Schema User: LIBRARY_USER
   ============================================================================= */

/* -----------------------------------------------------------------------------
   TABLE COMMENTS
   ----------------------------------------------------------------------------- */
COMMENT ON TABLE BookType     IS 'Categorization and genre classifications for books';
COMMENT ON TABLE Author       IS 'Registered authors of library books, biographical information, and photos';
COMMENT ON TABLE Student      IS 'Registered student library members permitted to borrow materials';
COMMENT ON TABLE Librarian    IS 'Library staff personnel who process borrows, returns, and cataloging';
COMMENT ON TABLE APP_USER     IS 'Application credentials (1-to-1 with Librarian) for system login';
COMMENT ON TABLE Book         IS 'Catalog of library book titles, physical copies, and publisher metadata';
COMMENT ON TABLE BookAuthor   IS 'Junction table supporting many-to-many relationship between books and authors';
COMMENT ON TABLE Borrow       IS 'Transaction master record representing a borrowing session';
COMMENT ON TABLE BorrowBook   IS 'Transaction detail record tracking individual books and quantities borrowed';
COMMENT ON TABLE BOOK_RETURN  IS 'Transaction record tracking returned book copies and receiving librarian';

/* -----------------------------------------------------------------------------
   COLUMN COMMENTS
   ----------------------------------------------------------------------------- */
COMMENT ON COLUMN Book.NumCopies         IS 'Number of physical copies currently on shelf available for loan';
COMMENT ON COLUMN BorrowBook.IsReturned  IS 'Return status flag: 0 = Out on loan, 1 = Fully returned';
COMMENT ON COLUMN BorrowBook.QtyBorrow   IS 'Quantity of copies borrowed in this transaction';
COMMENT ON COLUMN BOOK_RETURN.QtyReturn  IS 'Quantity of copies returned in this transaction';
COMMENT ON COLUMN APP_USER.UserType      IS 'User role designation: Admin, Librarian, or Staff';
COMMENT ON COLUMN Student.Photo          IS 'BLOB storage for student identification photograph';
COMMENT ON COLUMN Author.Photo           IS 'BLOB storage for author portrait photograph';

/* -----------------------------------------------------------------------------
   SCHEMA HEALTH VERIFICATION QUERY
   Check for any invalid stored procedures, triggers, views, or packages.
   If everything compiled cleanly, this query should return 0 rows.
   ----------------------------------------------------------------------------- */
COLUMN object_name FORMAT A30;
COLUMN object_type FORMAT A20;
COLUMN status FORMAT A10;

SELECT 
    object_name,
    object_type,
    status
FROM user_objects
WHERE status <> 'VALID'
ORDER BY object_type, object_name;

/* -----------------------------------------------------------------------------
   COMPILATION ERROR CHECK
   Shows line and position details if any PL/SQL object had compilation errors.
   ----------------------------------------------------------------------------- */
SELECT name, type, sequence, line, position, text
FROM user_errors
ORDER BY name, sequence;

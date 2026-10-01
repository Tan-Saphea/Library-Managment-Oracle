SET SQLBLANKLINES ON;

/* =============================================================================
   SCRIPT 03: DATABASE TRIGGERS
   Project: Library Management System
   DBMS: Oracle Database 23ai / 26ai Free (Docker)
   Schema User: LIBRARY_USER
   ============================================================================= */

/* -----------------------------------------------------------------------------
   TRIGGER 1: TRG_BORROWBOOK_VALIDATE_STOCK
   Timing: BEFORE INSERT ON BorrowBook FOR EACH ROW
   Purpose:
     Validates that the requested quantity to borrow does not exceed the current
     available stock (NumCopies) in the Book table.
     If insufficient copies exist, an application error is raised immediately,
     aborting the transaction and preventing negative stock.
   ----------------------------------------------------------------------------- */
CREATE OR REPLACE TRIGGER TRG_BORROWBOOK_VALIDATE_STOCK
BEFORE INSERT ON BorrowBook
FOR EACH ROW
DECLARE
    v_available_copies NUMBER;
BEGIN
    SELECT NumCopies
    INTO v_available_copies
    FROM Book
    WHERE BookID = :NEW.BookID;

    IF :NEW.QtyBorrow > v_available_copies THEN
        RAISE_APPLICATION_ERROR(
            -20001,
            'Not enough book copies available. Requested: ' || :NEW.QtyBorrow || ', Available: ' || v_available_copies
        );
    END IF;
EXCEPTION
    WHEN NO_DATA_FOUND THEN
        RAISE_APPLICATION_ERROR(
            -20002,
            'Invalid Book ID: ' || :NEW.BookID || ' does not exist.'
        );
END;
/

/* -----------------------------------------------------------------------------
   TRIGGER 2: TRG_BORROWBOOK_DECREASE_STOCK
   Timing: AFTER INSERT ON BorrowBook FOR EACH ROW
   Purpose:
     Automatically decreases Book.NumCopies by the borrowed quantity (QtyBorrow)
     after the borrow record is successfully inserted and verified.
   ----------------------------------------------------------------------------- */
CREATE OR REPLACE TRIGGER TRG_BORROWBOOK_DECREASE_STOCK
AFTER INSERT ON BorrowBook
FOR EACH ROW
BEGIN
    UPDATE Book
    SET NumCopies = NumCopies - :NEW.QtyBorrow
    WHERE BookID = :NEW.BookID;
END;
/

/* -----------------------------------------------------------------------------
   TRIGGER 3: TRG_RETURNBOOK_INCREASE_STOCK
   Timing: AFTER INSERT ON BOOK_RETURN FOR EACH ROW
   Purpose:
     Automatically restores Book.NumCopies by adding the returned quantity (QtyReturn)
     when a return record is inserted into BOOK_RETURN.
   ----------------------------------------------------------------------------- */
CREATE OR REPLACE TRIGGER TRG_RETURNBOOK_INCREASE_STOCK
AFTER INSERT ON BOOK_RETURN
FOR EACH ROW
BEGIN
    UPDATE Book
    SET NumCopies = NumCopies + :NEW.QtyReturn
    WHERE BookID = :NEW.BookID;
END;
/

/* -----------------------------------------------------------------------------
   TRIGGER EXECUTION FLOW & DESIGN NOTES (Mutating Table Error Prevention):
   1. Borrow Flow:
      a. SP_BORROWBOOK_INSERT inserts into BorrowBook.
      b. TRG_BORROWBOOK_VALIDATE_STOCK (BEFORE) reads Book.NumCopies. If QtyBorrow > NumCopies,
         it raises ORA-20001 and halts.
      c. Row is inserted into BorrowBook.
      d. TRG_BORROWBOOK_DECREASE_STOCK (AFTER) updates Book.NumCopies = NumCopies - QtyBorrow.
   2. Return Flow:
      a. SP_RETURN_BOOK validates return quantity against original borrow quantity.
      b. SP_RETURN_BOOK inserts into BOOK_RETURN.
      c. TRG_RETURNBOOK_INCREASE_STOCK (AFTER) updates Book.NumCopies = NumCopies + QtyReturn.
      d. SP_RETURN_BOOK checks if total returned >= borrowed; if true, sets BorrowBook.IsReturned = 1.
      * Note: Calculating total returns inside a row-level trigger on BOOK_RETURN would query
        BOOK_RETURN itself, causing Oracle error ORA-04091 (table is mutating). Managing the return
        completion status in SP_RETURN_BOOK ensures transactional safety without mutating errors.
   ----------------------------------------------------------------------------- */

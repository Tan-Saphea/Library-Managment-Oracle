SET DEFINE OFF;
SET SERVEROUTPUT ON SIZE UNLIMITED;
SET LINESIZE 200;
SET PAGESIZE 100;
SET SQLBLANKLINES ON;

/* =============================================================================
   SCRIPT 07: COMPREHENSIVE DATABASE VERIFICATION & TEST SUITE
   Project: Library Management System
   DBMS: Oracle Database 23ai / 26ai Free (Docker)
   Schema User: LIBRARY_USER
   ============================================================================= */

PROMPT =========================================================================
PROMPT TEST 1: VERIFY RECORD COUNTS IN ALL TABLES
PROMPT =========================================================================
SELECT 'BookType' AS TableName, COUNT(*) AS TotalRows FROM BookType UNION ALL
SELECT 'Author', COUNT(*) FROM Author UNION ALL
SELECT 'Student', COUNT(*) FROM Student UNION ALL
SELECT 'Librarian', COUNT(*) FROM Librarian UNION ALL
SELECT 'APP_USER', COUNT(*) FROM APP_USER UNION ALL
SELECT 'Book', COUNT(*) FROM Book UNION ALL
SELECT 'BookAuthor', COUNT(*) FROM BookAuthor UNION ALL
SELECT 'Borrow', COUNT(*) FROM Borrow UNION ALL
SELECT 'BorrowBook', COUNT(*) FROM BorrowBook UNION ALL
SELECT 'BOOK_RETURN', COUNT(*) FROM BOOK_RETURN;

PROMPT
PROMPT =========================================================================
PROMPT TEST 2: STUDENT CRUD STORED PROCEDURES
PROMPT =========================================================================
DECLARE
    v_new_stu_id  NUMBER;
    v_cursor      SYS_REFCURSOR;
    v_id          NUMBER;
    v_name        VARCHAR2(100);
    v_gender      VARCHAR2(10);
    v_dob         DATE;
    v_pob         VARCHAR2(150);
    v_address     VARCHAR2(255);
    v_phone       VARCHAR2(30);
    v_email       VARCHAR2(100);
    v_photo       BLOB;
BEGIN
    -- 2.1 Test Insert
    SP_STUDENT_INSERT(
        p_stu_name => 'Test Student Candidate',
        p_gender   => 'Female',
        p_dob      => TO_DATE('2004-01-15', 'YYYY-MM-DD'),
        p_pob      => 'Kampot',
        p_address  => 'St. 101, Phnom Penh',
        p_phone    => '011-998-877',
        p_email    => 'test.student@univ.edu',
        p_photo    => NULL,
        p_new_id   => v_new_stu_id
    );
    DBMS_OUTPUT.PUT_LINE('[x] SP_STUDENT_INSERT: Success. Generated StuID = ' || v_new_stu_id);

    -- 2.2 Test Select By ID
    SP_STUDENT_SELECT_BY_ID(p_stu_id => v_new_stu_id, p_cursor => v_cursor);
    FETCH v_cursor INTO v_id, v_name, v_gender, v_dob, v_pob, v_address, v_phone, v_email, v_photo;
    CLOSE v_cursor;
    DBMS_OUTPUT.PUT_LINE('[x] SP_STUDENT_SELECT_BY_ID: Retrieved ' || v_name || ' (' || v_gender || ')');

    -- 2.3 Test Update
    SP_STUDENT_UPDATE(
        p_stu_id   => v_new_stu_id,
        p_stu_name => 'Test Student (Updated)',
        p_gender   => 'Female',
        p_dob      => TO_DATE('2004-01-15', 'YYYY-MM-DD'),
        p_pob      => 'Kampot',
        p_address  => 'St. 202, Toul Kork, Phnom Penh',
        p_phone    => '011-998-877',
        p_email    => 'test.updated@univ.edu',
        p_photo    => NULL
    );
    DBMS_OUTPUT.PUT_LINE('[x] SP_STUDENT_UPDATE: Successfully updated student details');

    -- 2.4 Test Search
    SP_STUDENT_SEARCH(p_keyword => 'Updated', p_cursor => v_cursor);
    FETCH v_cursor INTO v_id, v_name, v_gender, v_dob, v_pob, v_address, v_phone, v_email, v_photo;
    CLOSE v_cursor;
    DBMS_OUTPUT.PUT_LINE('[x] SP_STUDENT_SEARCH: Found matching student ID ' || v_id || ' - ' || v_name);

    -- 2.5 Test Delete
    SP_STUDENT_DELETE(p_stu_id => v_new_stu_id);
    DBMS_OUTPUT.PUT_LINE('[x] SP_STUDENT_DELETE: Successfully deleted test student ID ' || v_new_stu_id);
END;
/

PROMPT
PROMPT =========================================================================
PROMPT TEST 3: AUTHOR CRUD STORED PROCEDURES
PROMPT =========================================================================
DECLARE
    v_new_auth_id NUMBER;
    v_cursor      SYS_REFCURSOR;
    v_id          NUMBER;
    v_name        VARCHAR2(100);
    v_gender      VARCHAR2(10);
    v_dob         DATE;
    v_pob         VARCHAR2(150);
    v_address     VARCHAR2(255);
    v_phone       VARCHAR2(30);
    v_email       VARCHAR2(100);
    v_photo       BLOB;
BEGIN
    -- 3.1 Test Insert
    SP_AUTHOR_INSERT(
        p_author_name => 'Test Author Prototype',
        p_gender      => 'Male',
        p_dob         => TO_DATE('1975-06-20', 'YYYY-MM-DD'),
        p_pob         => 'London, UK',
        p_address     => 'Cambridge, UK',
        p_phone       => '+44-20-7946-0919',
        p_email       => 'test.author@authors.org',
        p_photo       => NULL,
        p_new_id      => v_new_auth_id
    );
    DBMS_OUTPUT.PUT_LINE('[x] SP_AUTHOR_INSERT: Success. Generated AuthorID = ' || v_new_auth_id);

    -- 3.2 Test Select By ID
    SP_AUTHOR_SELECT_BY_ID(p_author_id => v_new_auth_id, p_cursor => v_cursor);
    FETCH v_cursor INTO v_id, v_name, v_gender, v_dob, v_pob, v_address, v_phone, v_email, v_photo;
    CLOSE v_cursor;
    DBMS_OUTPUT.PUT_LINE('[x] SP_AUTHOR_SELECT_BY_ID: Retrieved ' || v_name);

    -- 3.3 Test Update
    SP_AUTHOR_UPDATE(
        p_author_id   => v_new_auth_id,
        p_author_name => 'Test Author (Updated)',
        p_gender      => 'Male',
        p_dob         => TO_DATE('1975-06-20', 'YYYY-MM-DD'),
        p_pob         => 'London, UK',
        p_address     => 'Oxford, UK',
        p_phone       => '+44-20-7946-0919',
        p_email       => 'test.author.updated@authors.org',
        p_photo       => NULL
    );
    DBMS_OUTPUT.PUT_LINE('[x] SP_AUTHOR_UPDATE: Successfully updated author details');

    -- 3.4 Test Search
    SP_AUTHOR_SEARCH(p_keyword => 'Oxford', p_cursor => v_cursor);
    FETCH v_cursor INTO v_id, v_name, v_gender, v_dob, v_pob, v_address, v_phone, v_email, v_photo;
    CLOSE v_cursor;
    DBMS_OUTPUT.PUT_LINE('[x] SP_AUTHOR_SEARCH: Found matching author ID ' || v_id || ' - ' || v_name);

    -- 3.5 Test Delete
    SP_AUTHOR_DELETE(p_author_id => v_new_auth_id);
    DBMS_OUTPUT.PUT_LINE('[x] SP_AUTHOR_DELETE: Successfully deleted test author ID ' || v_new_auth_id);
END;
/

PROMPT
PROMPT =========================================================================
PROMPT TEST 4: BOOK CRUD STORED PROCEDURES (JOIN WITH BOOKTYPE)
PROMPT =========================================================================
DECLARE
    v_new_book_id NUMBER;
    v_type_id     NUMBER;
    v_cursor      SYS_REFCURSOR;
    v_id          NUMBER;
    v_title       VARCHAR2(200);
    v_tid         NUMBER;
    v_tname       VARCHAR2(100);
    v_pubdate     DATE;
    v_pages       NUMBER;
    v_copies      NUMBER;
    v_edition     VARCHAR2(50);
    v_pub         VARCHAR2(150);
    v_source      VARCHAR2(200);
    v_remark      VARCHAR2(500);
BEGIN
    SELECT BookTypeID INTO v_type_id FROM BookType WHERE ROWNUM = 1;

    -- 4.1 Test Insert
    SP_BOOK_INSERT(
        p_book_title   => 'Oracle Database 23ai Internal Architecture',
        p_book_type_id => v_type_id,
        p_publish_date => TO_DATE('2024-05-01', 'YYYY-MM-DD'),
        p_num_of_pages => 850,
        p_num_copies   => 10,
        p_edition      => '1st Edition',
        p_publisher    => 'Oracle Press',
        p_book_source  => 'University Library Fund',
        p_remark       => 'Benchmark test record',
        p_new_id       => v_new_book_id
    );
    DBMS_OUTPUT.PUT_LINE('[x] SP_BOOK_INSERT: Success. Generated BookID = ' || v_new_book_id);

    -- 4.2 Test Select By ID (Verifying join with BookType)
    SP_BOOK_SELECT_BY_ID(p_book_id => v_new_book_id, p_cursor => v_cursor);
    FETCH v_cursor INTO v_id, v_title, v_tid, v_tname, v_pubdate, v_pages, v_copies, v_edition, v_pub, v_source, v_remark;
    CLOSE v_cursor;
    DBMS_OUTPUT.PUT_LINE('[x] SP_BOOK_SELECT_BY_ID: Retrieved "' || v_title || '" | Genre: ' || v_tname || ' | Copies: ' || v_copies);

    -- 4.3 Test Update
    SP_BOOK_UPDATE(
        p_book_id      => v_new_book_id,
        p_book_title   => 'Oracle Database 23ai Internal Architecture (Revised)',
        p_book_type_id => v_type_id,
        p_publish_date => TO_DATE('2024-05-01', 'YYYY-MM-DD'),
        p_num_of_pages => 880,
        p_num_copies   => 12,
        p_edition      => '2nd Edition',
        p_publisher    => 'Oracle Press',
        p_book_source  => 'University Library Fund',
        p_remark       => 'Updated test record'
    );
    DBMS_OUTPUT.PUT_LINE('[x] SP_BOOK_UPDATE: Successfully updated book details');

    -- 4.4 Test Search
    SP_BOOK_SEARCH(p_keyword => 'Internal Architecture', p_cursor => v_cursor);
    FETCH v_cursor INTO v_id, v_title, v_tid, v_tname, v_pubdate, v_pages, v_copies, v_edition, v_pub, v_source, v_remark;
    CLOSE v_cursor;
    DBMS_OUTPUT.PUT_LINE('[x] SP_BOOK_SEARCH: Found matching book ID ' || v_id || ' - ' || v_title);

    -- 4.5 Test Delete
    SP_BOOK_DELETE(p_book_id => v_new_book_id);
    DBMS_OUTPUT.PUT_LINE('[x] SP_BOOK_DELETE: Successfully deleted test book ID ' || v_new_book_id);
END;
/

PROMPT
PROMPT =========================================================================
PROMPT TEST 5: VIEWS VERIFICATION
PROMPT =========================================================================
SELECT COUNT(*) AS VW_Book_Details_Count FROM VW_BOOK_DETAILS;
SELECT COUNT(*) AS VW_Book_Author_Count FROM VW_BOOK_AUTHOR;

PROMPT
PROMPT =========================================================================
PROMPT TEST 6: BORROW TRANSACTION & INVENTORY TRIGGER DEDUCTION
PROMPT =========================================================================
DECLARE
    v_stu_id        NUMBER;
    v_lib_id        NUMBER;
    v_book_id       NUMBER;
    v_stock_before  NUMBER;
    v_stock_after   NUMBER;
    v_borrow_id     NUMBER;
BEGIN
    SELECT StuID INTO v_stu_id FROM Student WHERE ROWNUM = 1;
    SELECT LibID INTO v_lib_id FROM Librarian WHERE ROWNUM = 1;
    SELECT BookID, NumCopies INTO v_book_id, v_stock_before FROM Book WHERE NumCopies >= 5 AND ROWNUM = 1;

    DBMS_OUTPUT.PUT_LINE('Starting Test 6: Book ID ' || v_book_id || ' initial stock = ' || v_stock_before);

    -- 6.1 Create Borrow Header
    SP_BORROW_INSERT(
        p_stu_id      => v_stu_id,
        p_lib_id      => v_lib_id,
        p_borrow_date => SYSDATE,
        p_remark      => 'Test Borrow Session',
        p_new_id      => v_borrow_id
    );
    DBMS_OUTPUT.PUT_LINE('[x] SP_BORROW_INSERT: Created Borrow ID = ' || v_borrow_id);

    -- 6.2 Borrow 2 copies (Triggers TRG_BORROWBOOK_VALIDATE_STOCK & TRG_BORROWBOOK_DECREASE_STOCK)
    SP_BORROWBOOK_INSERT(
        p_borrow_id  => v_borrow_id,
        p_book_id    => v_book_id,
        p_qty_borrow => 2
    );

    SELECT NumCopies INTO v_stock_after FROM Book WHERE BookID = v_book_id;
    DBMS_OUTPUT.PUT_LINE('[x] SP_BORROWBOOK_INSERT: Borrowed 2 copies. Stock after borrow = ' || v_stock_after);

    IF v_stock_after = (v_stock_before - 2) THEN
        DBMS_OUTPUT.PUT_LINE('[PASSED] Trigger TRG_BORROWBOOK_DECREASE_STOCK reduced inventory correctly.');
    ELSE
        DBMS_OUTPUT.PUT_LINE('[FAILED] Stock count mismatch!');
    END IF;

    -- 6.3 Test 8: Invalid borrowing when there is not enough stock
    BEGIN
        DBMS_OUTPUT.PUT_LINE('Testing over-borrow validation (requesting 999 copies)...');
        SP_BORROWBOOK_INSERT(
            p_borrow_id  => v_borrow_id,
            p_book_id    => v_book_id,
            p_qty_borrow => 999
        );
        DBMS_OUTPUT.PUT_LINE('[FAILED] Trigger allowed over-borrowing!');
    EXCEPTION
        WHEN OTHERS THEN
            DBMS_OUTPUT.PUT_LINE('[PASSED] Over-borrow blocked by TRG_BORROWBOOK_VALIDATE_STOCK: ' || SQLERRM);
    END;

    -- 6.4 Test 9 & 10: Return Book & Verify Stock Restored
    DECLARE
        v_ret_id NUMBER;
    BEGIN
        SP_RETURN_BOOK(
            p_borrow_id     => v_borrow_id,
            p_book_id       => v_book_id,
            p_lib_id        => v_lib_id,
            p_qty_return    => 2,
            p_remark        => 'Full return test',
            p_new_return_id => v_ret_id
        );
        DBMS_OUTPUT.PUT_LINE('[x] SP_RETURN_BOOK: Successfully returned 2 copies. Return ID = ' || v_ret_id);

        SELECT NumCopies INTO v_stock_after FROM Book WHERE BookID = v_book_id;
        DBMS_OUTPUT.PUT_LINE('[x] Stock after return = ' || v_stock_after);

        IF v_stock_after = v_stock_before THEN
            DBMS_OUTPUT.PUT_LINE('[PASSED] Trigger TRG_RETURNBOOK_INCREASE_STOCK restored stock to original ' || v_stock_before);
        ELSE
            DBMS_OUTPUT.PUT_LINE('[FAILED] Stock not restored accurately!');
        END IF;
    END;

    -- Verify Views after transactions
    SELECT COUNT(*) INTO v_stock_after FROM VW_BORROW_DETAILS WHERE BorrowID = v_borrow_id;
    DBMS_OUTPUT.PUT_LINE('[x] VW_BORROW_DETAILS verified (' || v_stock_after || ' records found).');

    SELECT COUNT(*) INTO v_stock_after FROM VW_RETURN_DETAILS WHERE BorrowID = v_borrow_id;
    DBMS_OUTPUT.PUT_LINE('[x] VW_RETURN_DETAILS verified (' || v_stock_after || ' records found).');
END;
/

PROMPT
PROMPT =========================================================================
PROMPT TEST 7: LOGIN STORED PROCEDURE
PROMPT =========================================================================
DECLARE
    v_cursor   SYS_REFCURSOR;
    v_lib_id   NUMBER;
    v_user     VARCHAR2(100);
    v_role     VARCHAR2(30);
    v_name     VARCHAR2(100);
    v_email    VARCHAR2(100);
BEGIN
    -- Valid Login (admin / 123)
    SP_LOGIN(
        p_username => 'admin',
        p_password => '123',
        p_cursor   => v_cursor
    );
    FETCH v_cursor INTO v_lib_id, v_user, v_role, v_name, v_email;
    IF v_cursor%FOUND THEN
        DBMS_OUTPUT.PUT_LINE('[PASSED] Valid Login: Authenticated ' || v_name || ' as Role: ' || v_role);
    ELSE
        DBMS_OUTPUT.PUT_LINE('[FAILED] Valid login failed to authenticate!');
    END IF;
    CLOSE v_cursor;

    -- Invalid Login Check
    SP_LOGIN(
        p_username => 'admin',
        p_password => 'WrongPassword!',
        p_cursor   => v_cursor
    );
    FETCH v_cursor INTO v_lib_id, v_user, v_role, v_name, v_email;
    IF v_cursor%NOTFOUND THEN
        DBMS_OUTPUT.PUT_LINE('[PASSED] Invalid credentials correctly rejected.');
    ELSE
        DBMS_OUTPUT.PUT_LINE('[FAILED] Invalid password accepted!');
    END IF;
    CLOSE v_cursor;
END;
/

PROMPT
PROMPT =========================================================================
PROMPT TEST 8: ALL DATABASE OBJECTS STATUS
PROMPT =========================================================================
COLUMN object_name FORMAT A30;
COLUMN object_type FORMAT A20;
COLUMN status FORMAT A10;

SELECT 
    object_type,
    COUNT(*) AS total_count,
    SUM(CASE WHEN status = 'VALID' THEN 1 ELSE 0 END) AS valid_count,
    SUM(CASE WHEN status <> 'VALID' THEN 1 ELSE 0 END) AS invalid_count
FROM user_objects
GROUP BY object_type
ORDER BY object_type;

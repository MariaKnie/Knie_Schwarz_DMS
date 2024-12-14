@echo off

REM --------------------------------------------------
REM DMS - Integration Tests
REM --------------------------------------------------
title KS DMS
echo CURL Testing for KS DMS
echo[

REM --------------------------------------------------
echo 1) Upload of a document
REM Create MyDoc item to hold a document
curl -X POST ^
  "http://localhost:8081/mydoc" ^
  -H "accept: */*" ^
  -H "Content-Type: application/json" ^
  -d "{\"id\": 100, \"title\": \"testTitle\", \"author\": \"testAuthor\", \"createddate\": \"2024-12-14T08:47:24.799Z\", \"editeddate\": \"2024-12-14T08:47:24.799Z\", \"textfield\": \"testText\", \"filename\": \"\", \"ocrtext\": \"\", \"itemDto\": {}}"
echo[
echo[
REM Create a second MyDoc item to hold a document
curl -X POST ^
  "http://localhost:8081/mydoc" ^
  -H "accept: */*" ^
  -H "Content-Type: application/json" ^
  -d "{\"id\": 101, \"title\": \"second\", \"author\": \"Maria\", \"createddate\": \"2024-12-14T08:47:24.799Z\", \"editeddate\": \"2024-12-14T08:47:24.799Z\", \"textfield\": \"Grossmeister\", \"filename\": \"\", \"ocrtext\": \"\", \"itemDto\": {}}"
echo[
echo[

REM --------------------------------------------------
echo Upload document 
REM Upload the document to both MyDoc items
curl -X PUT ^
  "http://localhost:8081/mydoc/100/upload" ^
  -H "accept: */*" ^
  -H "Content-Type: multipart/form-data" ^
  -F "docFile=@\"KS_DMS_Project\Tests\Test.pdf\";type=application/pdf"
echo[
echo[
curl -X PUT ^
  "http://localhost:8081/mydoc/101/upload" ^
  -H "accept: */*" ^
  -H "Content-Type: multipart/form-data" ^
  -F "docFile=@\"KS_DMS_Project\Tests\Test.pdf\";type=application/pdf"
echo[
echo[

REM --------------------------------------------------
echo Check for uploaded document/file
curl -X GET ^
  "http://localhost:8081/mydoc/100" ^
  -H "accept: */*"
echo[
echo[
curl -X GET ^
  "http://localhost:8081/mydoc/101" ^
  -H "accept: */*"
echo[
echo[

REM --------------------------------------------------
echo End of upload testing...
echo -------------------------------------------------
echo[
echo[

REM --------------------------------------------------
echo 2) Searching for a document
echo[
echo[
echo 2a) Searching for an author
REM Search for the author of the documents with wildcard search
curl -X POST ^
  "http://localhost:8081/mydoc/search/querystring" ^
  -H "accept: */*" ^
  -H "Content-Type: application/json" ^
  -d "\"test\""
echo[
echo[
curl -X POST ^
  "http://localhost:8081/mydoc/search/querystring" ^
  -H "accept: */*" ^
  -H "Content-Type: application/json" ^
  -d "\"Mar\""
echo[
echo[

echo 2b) Searching for a non existing document (should show not-found response)
REM Search for the document with fuzzy search
curl -X POST ^
  "http://localhost:8081/mydoc/search/fuzzy" ^
  -H "accept: */*" ^
  -H "Content-Type: application/json" ^
  -d "\"NotExisting\""
echo[
echo[

echo 2c) Searching with an empty string (should fail due to empty searchstring)
REM Search for the document with an empty string
curl -X POST ^
  "http://localhost:8081/mydoc/search/querystring" ^
  -H "accept: */*" ^
  -H "Content-Type: application/json" ^
  -d "\"\""
echo[
echo[

REM --------------------------------------------------
echo End of search testing...
echo -------------------------------------------------
echo[
echo[

REM --------------------------------------------------
echo 3) Deletion of a document
REM Delete MyDoc items holding a document
curl -X DELETE ^
  "http://localhost:8081/mydoc/100" ^
  -H "accept: */*"
echo[
echo[

curl -X DELETE ^
  "http://localhost:8081/mydoc/101" ^
  -H "accept: */*"
echo[
echo[

echo Check for the deleted document/file (should fail)
curl -X GET ^
  "http://localhost:8081/mydoc/100" ^
  -H "accept: */*"
echo[
echo[


REM --------------------------------------------------
echo End of deletion testing...
echo -------------------------------------------------
echo[
echo[

REM Sleep for approx 30 seconds (ping localhost)
echo End of Integration testing (Sleeping for 30 sec to visualize results)
echo[
ping localhost -n  31>NUL 2>NUL
@echo on

Tilde TAWS
===

Showcase
---

The Web-based showcase provides a graphical user interface (GUI) for terminology annotation in ITS 2.0 enriched documents. 

The showcase is accessible from the Web page http://taws.tilde.com/. 

The showcase allows submitting plaintext, HTML5, and XLIFF documents for automatic terminology annotation. 

The user uploaded (or submitted) documents are forwarded to the Terminology Annotation Web Service (TAWS) that performs automatic terminology annotation. 

The results of the terminology annotation are presented to the user in a Web Frame and the annotated document can also be downloaded. 

API
---

TAWS exposes a RESTful API over HTTP.

**HTML5**

Request

    POST /api/html5 HTTP/1.1
    Host: taws.tilde.com
    Content-Length: 62

    <!DOCTYPE html><html lang="en"><body>hello world</body></html>

Response

    HTTP/1.1 200 OK
    Content-Type: text/html; charset=utf-8

    <!DOCTYPE html>
    <html lang="en">
    <body its-annotators-ref="terminology|http://tilde.com/term-annotation-service">
      <span its-term="yes" its-term-confidence="1">hello world</span>
    </body>
    </html>

More information and examples are available at [/about](/about) on your local site, [on taws.tilde.com](http://taws.tilde.com/about) or [here in this repository](Tilde.Taws/Views/Showcase/About.cshtml).
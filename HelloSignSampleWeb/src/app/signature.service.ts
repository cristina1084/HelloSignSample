import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
@Injectable({
  providedIn: 'root'
})
export class SignatureService {

  constructor(private http: HttpClient) { }

  uploadFile(file: File): Observable<any> {
    const fileInput = new FormData();
    fileInput.append('file', file);
    return this.http.post('http://localhost:5000/api/Esignature/upload', fileInput);
  }

  createDraftTemplate(leaseData): Observable<any> {
    return this.http.post('http://localhost:5000/api/Esignature/create', leaseData);
  }

  getTemplates(): Observable<any> {
    return this.http.get('http://localhost:5000/api/Esignature/templates');
  }

  editTemplate(templateId): Observable<any> {
    return this.http.get('http://localhost:5000/api/Esignature/template/' + templateId);
  }

  send(requestData): Observable<any> {
    return this.http.post('http://localhost:5000/api/Esignature/send', requestData);
  }

  sign(signatureId): Observable<any> {
    return this.http.get('http://localhost:5000/api/Esignature/sign/' + signatureId);
  }

  view(requestId): Observable<any> {
    // const httpOptions = {
    //   responseType  : 'arraybuffer' as 'json'
    //    // 'responseType'  : 'blob' as 'json'        //This also worked
    // };
    return this.http.get('http://localhost:5000/api/Esignature/view/' + requestId, { responseType: 'blob' });
  }

  getSignatureRequests(): Observable<any> {
    return this.http.get('http://localhost:5000/api/Esignature/signatureRequests');
  }
}

import { Component, OnInit, ViewChild } from '@angular/core';
import { SignatureService } from './signature.service';
import * as HelloSign from 'hellosign-embedded';
import { saveAs } from 'file-saver';
import { FormGroup, FormBuilder, FormControl, FormArray } from '@angular/forms';
import { MessageService } from 'primeng/api';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css'],
  providers: [MessageService]
})
export class AppComponent implements OnInit {
  @ViewChild('fileInput', {static: false}) fileInput;
  file: File;
  public staged = false;
  title = 'HelloSignSampleWeb';
  clientId = 'xxxxxxxxxxxxxxxxxxxxxxxxx';
  data: any;
  templates: any;
  signatureRequestId: any;
  leaseForm: FormGroup;

// create form according to template
  formTemplate = [
    // {name: 'Address', controlName: 'address', type: 'text'}
  ];
  templateId: any;
  customFields: FormArray;
  signatureRequests: any;

  constructor(private signatureService: SignatureService, private fb: FormBuilder, private messageService: MessageService) {
  }

  ngOnInit() {
    const group = {};
    this.formTemplate.forEach(input => {
      group[input.controlName] = new FormControl('');
    });
    this.leaseForm = this.fb.group({
      customFields: this.fb.array([])
    });
    this.customFields = this.leaseForm.get('customFields') as FormArray;
    this.formTemplate.forEach(input => {
      this.customFields.push(this.createCustomField(input.name));
    });
  }

  createCustomField(fieldName): FormGroup {
    return this.fb.group({
      name: [fieldName],
      value: ['']
    });
  }

  stageFile() {
    this.staged = true;
    this.file = this.fileInput.nativeElement.files[0];
  }

  createTemplate() {
    if (this.file != null) {
      this.signatureService.uploadFile(this.file).subscribe(res => {
        const leaseData = {
          formFieldName: this.formTemplate,
          filePath: res.data
        };
        this.signatureService.createDraftTemplate(leaseData).subscribe(result => {
          this.templateId = result.data.templateId;
          const client = new HelloSign({
            clientId: this.clientId
          });
          client.open(result.data.editUrl,
          {
            skipDomainVerification: true,
            // container: document.getElementById('demo')
          });
          client.on('finish', e => {
            this.messageService.add({severity: 'success', summary: 'Templated created successfully'});
          });
          // this.getTemplates();
        });
      });
    }
  }

  getTemplates() {
    this.signatureService.getTemplates().subscribe(res => {
      this.templates = res.data;
    });
  }

  editTemplate(id) {
    this.signatureService.editTemplate(id).subscribe(res => {
      const client = new HelloSign({
        clientId: this.clientId
      });
      client.open(res.data.editUrl,
        {
          skipDomainVerification: true,
          // container: document.getElementById('demo')
        });
      // window.scrollTo({
      //   top: document.getElementById('demo-container').offsetTop,
      //   behavior: 'smooth',
      // });
    });
  }

  sendTemplate(id) {
    const requestData = this.leaseForm.value;
    requestData.templateId = id;
    this.signatureService.send(requestData).subscribe(res => {
      this.messageService.add({severity: 'success', summary: 'Document send out for signature'});
    });
  }

  signTemplate(signatureId) {
    this.signatureService.sign(signatureId).subscribe(res => {
      const client = new HelloSign({
        clientId: this.clientId
      });
      client.open(res.data.signUrl,
      {
        skipDomainVerification: true,
        // container: document.getElementById('demo')
      });
      client.on('finish', e => {
        this.signatureRequest();
        this.messageService.add({severity: 'success', summary: 'Document signed'});
      });
    });
  }

  viewSignedTemplate(requestId) {
    this.signatureService.view(requestId).subscribe(res => {
      // const file = new Blob([res], { type: 'application/pdf' });
      // console.log(file);
      saveAs(res, 'sample.pdf');
      // const fileURL = URL.createObjectURL(file);
      // window.open(fileURL);
    });
  }

  signatureRequest() {
    this.signatureService.getSignatureRequests().subscribe(res => {
      this.signatureRequests = res.data;
      this.signatureRequestId = res.data[0].signatureRequestId;
    });
  }

}

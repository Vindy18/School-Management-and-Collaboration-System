
import { AuthService } from 'src/app/core/service/auth.service';
import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MessageService } from 'primeng/api';
@Component({
  selector: 'app-signin',
  templateUrl: './signin.component.html',
  styleUrls: ['./signin.component.scss'],
  providers: [MessageService]
})
export class SigninComponent implements OnInit {
  loginForm: FormGroup;
  submitted = false;
  returnUrl: string;
  error = 'Username and Password not valid !';
  hide = true;
  constructor(
    private formBuilder: FormBuilder,
    private router: Router,
    private authService: AuthService,
    private messageService: MessageService
  ) {}
  ngOnInit() {
    this.loginForm = this.formBuilder.group({
      schoolDomain:['', Validators.required],
      username: ['', Validators.required],
      password: ['', Validators.required],
    });
  }
  get f() {
    return this.loginForm.controls;
  }
  onSubmit() {
    this.submitted = true;
    this.error = '';

    if (this.loginForm.invalid) {
      //this.error = 'Username and Password not valid !';
      this.messageService.add({severity:'success', summary: 'Success', detail: 'Message Content'});
      
      
      return;
    } else {
      this.authService
        .login(this.loginForm.value)
        .subscribe(
          (res) => {
            if (res) {
              const token = this.authService.currentUserValue.token;
              if (token) {
                this.router.navigate(['/teacher-home/lessons']);
              }
            } else {
              this.error = 'Invalid Login';
            }
          },
          (error) => {
            this.error = error;
            this.submitted = false;
          }
        );
    }
  }
}

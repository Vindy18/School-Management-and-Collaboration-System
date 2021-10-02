import { classnameModel } from './../../../models/class-name/classname.model';
import { ToastrService } from 'ngx-toastr';
import { ClassNameService } from './../../../services/classname/classname.service';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { FormGroup, FormBuilder, Validators, FormControl } from '@angular/forms';
import { DatatableComponent } from '@swimlane/ngx-datatable';
import { Component, OnInit, ViewChild } from '@angular/core';
import Swal from 'sweetalert2';
import { NgxSpinnerService } from 'ngx-spinner';

@Component({
  selector: 'app-class-name-list',
  templateUrl: './class-name-list.component.html',
  styleUrls: ['./class-name-list.component.sass'],
  providers: [ToastrService],
})
export class ClassNameListComponent implements OnInit {

  @ViewChild(DatatableComponent, { static: false }) table: DatatableComponent;
  data = [];
  scrollBarHorizontal = window.innerWidth < 1200;
  loadingIndicator = false;
  saveClassNameForm:FormGroup;
  classNameFilterForm:FormGroup;
  reorderable = true;
  classname:classnameModel
  currentPage: number = 0;
  pageSize: number = 10;
  totalRecord: number = 0;

  constructor(
    private fb: FormBuilder,
    private modalService: NgbModal,
    private classnameService:ClassNameService,
    private spinner:NgxSpinnerService,
    private toastr: ToastrService) { }

  ngOnInit(): void {
    this.getAll();
    this.classNameFilterForm = this.createClassNameFilterForm();
  }

  //Retrive class name details
  getAll(){
    this.loadingIndicator=true;
    this.classnameService.getAll().subscribe(response=>
    {
      this.data= response;
      this.loadingIndicator=false;
    },error=>{
      this.spinner.hide();
      this.loadingIndicator=false;
    });
  }

  setPage(pageInfo) {
    this.spinner.show();
    this.loadingIndicator = true;
    this.currentPage = pageInfo.offset;
    this.getClassNameList();
  }

   //FIlter Master 
   filterDatatable(event) {
    this.currentPage = 0;
    this.pageSize = 25;
    this.totalRecord = 0;
    const val = event.target.value.toLowerCase();
    this.spinner.show();
    this.getClassNameList();
  }

  getClassNameList(){
    this.loadingIndicator = true;
    this.classnameService.getClassNameList(this.searchFilterdId, this.currentPage + 1, this.pageSize)
    .subscribe(response=>{
      this.data = response.data;
      this.totalRecord = response.totalRecordCount;
      this.spinner.hide();
      this.loadingIndicator = false;

    },error=>{
      this.spinner.hide();
      this.loadingIndicator = false;
      this.toastr.error("Network error has been occured. Please try again.","Error");
    });
  }

  createClassNameFilterForm():FormGroup{

    return new FormGroup({
      searchText:new FormControl(""),
    });
  }
   //getters
get searchFilterdId(){
  return this.classNameFilterForm.get("searchText").value
    
}

  //create new class name (Reactive Form)
  addNewClassName(content) {

    this.saveClassNameForm = this.fb.group({
      name: ['', [Validators.required]],
      description: ['', [Validators.required]],
    });

    this.modalService.open(content, {
      ariaLabelledBy: 'modal-basic-title',
      size: 'lg',
    });

  }

  //Save created class name
  saveClassName(){   
    
    console.log(this.saveClassNameForm.value);
    
    this.classnameService.saveClassName(this.saveClassNameForm.value)
    .subscribe(response=>{

        if(response.isSuccess)
        {
          this.modalService.dismissAll();
          this.toastr.success(response.message,"Success");
          this.getAll();
        }
        else
        {
          this.toastr.error(response.message,"Error");
        }

    },error=>{
      this.toastr.error("Network error has been occured. Please try again.","Error");
    });

  }

  onAddRowSave(form: FormGroup) {
    this.data.push(form.value);
    this.data = [...this.data];
    form.reset();
    this.modalService.dismissAll();
    this.addRecordSuccess();
  }

  //Update class name
  editRow(row:classnameModel, rowIndex:number, content:any) {

    console.log(row);

    this.saveClassNameForm = this.fb.group({
      id:[row.id],
      name: [row.name, [Validators.required]],
      description: [row.description,[Validators.required]],
    });

    this.modalService.open(content, {
      ariaLabelledBy: 'modal-basic-title',
      size: 'lg',
    });
  }

  //delete class name
  deleteClassName(row) {
    Swal.fire({
      title: 'Are you sure Delete Class Name ?',
      showCancelButton: true,
      confirmButtonColor: 'red',
      cancelButtonColor: 'green',
      confirmButtonText: 'Yes',
    }).then((result) => {
      if (result.value) {

        this.classnameService.delete(row.id).subscribe(response=>{

          if(response.isSuccess)
          {
            this.toastr.success(response.message,"Success");
            this.getAll();
          }
          else
          {
            this.toastr.error(response.message,"Error");
          }
    
        },error=>{
          this.toastr.error("Network error has been occured. Please try again.","Error");
        });
      }
    });
  }

  //Success save message
  addRecordSuccess() {
    this.toastr.success('ClassName Add Successfully', '');
  }
}

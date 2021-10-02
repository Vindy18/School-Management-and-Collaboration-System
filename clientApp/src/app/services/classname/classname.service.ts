import { ResponseModel } from '../../models/common/response.model';
import { environment } from '../../../environments/environment';
import { classnameModel } from '../../models/class-name/classname.model';
import { Observable } from 'rxjs';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { ClassNamePaginatedItemViewModel } from "src/app/models/class-name/classname.paginated.item.model";

@Injectable({
  providedIn: 'root'
})

export class ClassNameService {

  constructor(private httpClient: HttpClient) { }

  getClassNameList(searchText: string, currentPage: number, pageSize: number): Observable<ClassNamePaginatedItemViewModel> {
    return this.httpClient.get<ClassNamePaginatedItemViewModel>(environment.apiUrl + "ClassName/getClassNameList", {
      params: new HttpParams()
        .set('searchText', searchText)
        .set('currentPage', currentPage.toString())
        .set('pageSize', pageSize.toString())
    });
  }

  getAll(): Observable<classnameModel[]>{
    return this.httpClient.
      get<classnameModel[]>(environment.apiUrl + 'ClassName')
  }

  saveClassName(vm: classnameModel): Observable<ResponseModel> {
    return this.httpClient.
      post<ResponseModel>(environment.apiUrl + 'ClassName', vm);
  }

  delete(id: number): Observable<ResponseModel> {
    return this.httpClient.
      delete<ResponseModel>(environment.apiUrl + 'ClassName/' + id);
  }
  
}

import { BasicClassNametModel } from './basic.classname.model';
import { PaginatedItemsModel } from './../common/paginated.Items.model';
import { Injectable } from '@angular/core';

@Injectable()
export class ClassNamePaginatedItemViewModel extends PaginatedItemsModel
{
    data:BasicClassNametModel[];
}
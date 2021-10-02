import { Injectable } from '@angular/core';

@Injectable()
export class BasicClassNametModel
{
    id:number;
    name:string;
    description:string;
    isActive:boolean;
    createdOn:Date;
    createdByName:string;
    updatedOn:Date;
    updatedByName:string;
}
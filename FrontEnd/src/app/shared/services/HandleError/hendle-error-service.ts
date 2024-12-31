// import { HttpErrorResponse } from '@angular/common/http';
// import { Injectable } from '@angular/core';
// import { Observable, throwError } from 'rxjs';

// @Injectable({
//   providedIn: 'root'
// })
// export class HendleErrorService {

//   constructor() { }

//   logErrorResponse(errorResponse:HttpErrorResponse):Observable<any>{
//     if(errorResponse.status ===0)
//       {
//         console.log(`A client side error occurred:${errorResponse.status} - ${errorResponse.error.message}`)
//       }else{
//        console.log(`A back-end error occurred:${errorResponse.status} - ${errorResponse.error.message}`)
//       }
//       return throwError(()=> new Error('Something bad happened'));
      
//   }
// }

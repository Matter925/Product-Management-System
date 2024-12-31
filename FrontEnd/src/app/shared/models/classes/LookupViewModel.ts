export class LookupViewModel {
  id: number;
  name: string;
  orderIndex?: number;
  names?: {
    english: string;
    arabic: string;
  };

  constructor(id: number, name: string, orderIndex?: number, names?: { english: string; arabic: string }) {
    this.id = id;
    this.name = name;
    this.orderIndex = orderIndex;
    this.names = names;
  }
}

export class LookupWithImageViewModel{
id: number;
name: string;
image: string;
orderIndex?: number;
constructor(id: number, name: string,image:string, orderIndex?: number) {
  this.id = id;
  this.name = name;
  this.image = image;
  this.orderIndex = orderIndex;
}
}
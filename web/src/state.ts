import type {Product,OrderLine} from './api';
export type Basket=Readonly<Record<string,number>>;
export type Action={type:'add';product:Product}|{type:'remove';id:string}|{type:'clear'};
export function basketReducer(state:Basket,action:Action):Basket{
  switch(action.type){
    case 'clear':return {};
    case 'remove':{const next={...state};delete next[action.id];return next;}
    case 'add':{
      const current=state[action.product.id]??0;
      return current>=action.product.available||action.product.available<1?state:
        {...state,[action.product.id]:current+1};
    }
  }
}
export function orderLines(basket:Basket):OrderLine[]{
  return Object.entries(basket).filter(([,n])=>n>0).map(([productId,quantity])=>({productId,quantity}));
}
export function signature(lines:readonly OrderLine[]):string{
  return [...lines].sort((a,b)=>a.productId.localeCompare(b.productId))
    .map(line=>`${line.productId}:${line.quantity}`).join(';');
}

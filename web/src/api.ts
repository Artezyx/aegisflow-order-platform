export interface Product { id:string; sku:string; name:string; available:number; version:number }
export interface OrderLine { productId:string; quantity:number }
export interface OrderResult { id:string; createdAt:string; items:OrderLine[]; replayed:boolean }
export class RequestError extends Error { constructor(readonly status:number,message:string){super(message)} }
async function call<T>(url:string,init?:RequestInit):Promise<T>{
  const response=await fetch(url,{...init,headers:{Accept:'application/json',...init?.headers}});
  if(!response.ok){
    const payload:unknown=await response.json().catch(()=>null);
    const message=(payload && typeof payload==='object' && 'detail' in payload && typeof payload.detail==='string')?payload.detail:
      (payload && typeof payload==='object' && 'error' in payload && typeof payload.error==='string')?payload.error:
      `HTTP ${response.status}`;
    throw new RequestError(response.status,message);
  }
  return await response.json() as T;
}
export const api={
  products:()=>call<Product[]>('/api/products'),
  addProduct:(p:{sku:string;name:string;available:number})=>call<Product>('/api/products',
    {method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify(p)}),
  placeOrder:(lines:OrderLine[],idempotencyKey:string)=>call<OrderResult>('/api/orders',
    {method:'POST',headers:{'Content-Type':'application/json','Idempotency-Key':idempotencyKey},body:JSON.stringify({items:lines})})
};

import {afterEach,describe,it,expect,vi} from 'vitest';
import {cleanup,render,screen,fireEvent,waitFor} from '@testing-library/react';
import {App} from './App';
afterEach(()=>{cleanup();vi.unstubAllGlobals()});
describe('Flujo React a API REST',()=>{
  it('carga inventario y envía un pedido con Idempotency-Key',async()=>{
    const calls:Array<{url:string;init?:RequestInit}>=[];
    vi.stubGlobal('fetch',vi.fn(async(url:string,init?:RequestInit)=>{
      calls.push({url,init});
      if(url==='/api/products')return {ok:true,json:async()=>[{id:'d276cd56-726c-4e16-ac63-afabde87f358',sku:'A1',name:'Módulo',available:2,version:0}]};
      if(url==='/api/orders')return {ok:true,json:async()=>({id:'e045ca18-39f1-4699-a136-ea4234df98e0',createdAt:'2026-11-12T00:00:00Z',items:[],replayed:false})};
      throw Error('Unexpected endpoint');
    }));
    render(<App/>);
    expect(await screen.findByText('Módulo')).toBeInTheDocument();
    fireEvent.click(screen.getByRole('button',{name:'Agregar Módulo'}));
    expect(screen.getByText('Módulo × 1')).toBeInTheDocument();
    fireEvent.click(screen.getByRole('button',{name:'Confirmar pedido'}));
    await waitFor(()=>expect(calls.some(c=>c.url==='/api/orders')).toBe(true));
    const order=calls.find(c=>c.url==='/api/orders')!;
    expect(new Headers(order.init?.headers).get('Idempotency-Key')).toBeTruthy();
    expect(order.init?.method).toBe('POST');
    expect(await screen.findByText(/Pedido e045ca18/)).toBeInTheDocument();
  });
});

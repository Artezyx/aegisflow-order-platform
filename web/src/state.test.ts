import {describe,it,expect} from 'vitest';
import {basketReducer,orderLines,signature} from './state';
import type {Product} from './api';
const p:Product={id:'d276cd56-726c-4e16-ac63-afabde87f358',sku:'A1',name:'Módulo',available:2,version:0};
describe('Reglas del frontend',()=>{
  it('respeta las existencias al preparar un pedido',()=>{
    const a=basketReducer({}, {type:'add',product:p});
    const b=basketReducer(a, {type:'add',product:p});
    const c=basketReducer(b, {type:'add',product:p});
    expect(orderLines(c)).toEqual([{productId:p.id,quantity:2}]);
  });
  it('elimina una línea sin mutar el estado anterior',()=>{
    const initial={[p.id]:1};
    expect(orderLines(basketReducer(initial,{type:'remove',id:p.id}))).toEqual([]);
    expect(initial[p.id]).toBe(1);
  });
  it('firma idéntico payload independientemente del orden',()=>{
    const a=[{productId:'z',quantity:1},{productId:'a',quantity:2}];
    expect(signature(a)).toBe(signature([...a].reverse()));
    expect(signature(a)).not.toBe(signature([{productId:'a',quantity:3}]));
  });
});

import {useEffect,useMemo,useReducer,useRef,useState} from 'react';
import type {FormEvent} from 'react';
import {api,type Product} from './api';
import {basketReducer,orderLines,signature} from './state';

const message=(error:unknown)=>error instanceof Error?error.message:'Error no identificado';
export function App(){
  const [items,setItems]=useState<Product[]>([]);
  const [loading,setLoading]=useState(true);
  const [busy,setBusy]=useState(false);
  const [notice,setNotice]=useState<{error:boolean;text:string}|null>(null);
  const [sku,setSku]=useState('');
  const [name,setName]=useState('');
  const [quantity,setQuantity]=useState(1);
  const [basket,dispatch]=useReducer(basketReducer,{});
  const pending=useRef<{signature:string;key:string}|null>(null);
  const lines=useMemo(()=>orderLines(basket),[basket]);
  const available=useMemo(()=>items.reduce((sum,p)=>sum+p.available,0),[items]);
  const selected=useMemo(()=>lines.reduce((sum,l)=>sum+l.quantity,0),[lines]);
  async function refresh(){setItems(await api.products());}
  useEffect(()=>{
    let alive=true;
    api.products().then(p=>{if(alive)setItems(p)}).catch(e=>{if(alive)setNotice({error:true,text:message(e)})})
      .finally(()=>{if(alive)setLoading(false)});
    return()=>{alive=false};
  },[]);
  async function createProduct(e:FormEvent<HTMLFormElement>){
    e.preventDefault();
    if(!sku.trim()||!name.trim()||!Number.isInteger(quantity)||quantity<0){
      setNotice({error:true,text:'Ingresa datos válidos'});return;
    }
    setBusy(true);setNotice(null);
    try{
      await api.addProduct({sku:sku.trim(),name:name.trim(),available:quantity});
      await refresh();setSku('');setName('');setQuantity(1);
      setNotice({error:false,text:'Producto guardado'});
    }catch(e){setNotice({error:true,text:message(e)})}
    finally{setBusy(false)}
  }
  async function createOrder(){
    if(!lines.length)return;
    const currentSignature=signature(lines);
    if(!pending.current||pending.current.signature!==currentSignature){
      pending.current={signature:currentSignature,key:crypto.randomUUID()};
    }
    setBusy(true);setNotice(null);
    try{
      const order=await api.placeOrder(lines,pending.current.key);
      dispatch({type:'clear'});pending.current=null;
      await refresh();
      setNotice({error:false,text:`Pedido ${order.id} creado${order.replayed?' (reintento seguro)':''}`});
    }catch(e){setNotice({error:true,text:`Pedido no confirmado: ${message(e)}. Reintenta sin cambiar la selección.`})}
    finally{setBusy(false)}
  }
  return <main className="shell">
    <header><p className="eyebrow">PORTAFOLIO FULLSTACK · REACT + .NET</p><h1>AegisFlow</h1>
      <p>Control de inventario y pedidos con transacciones e idempotencia.</p></header>
    <div className="metrics" aria-label="Métricas de inventario">
      <div><small>Productos</small><b>{items.length}</b></div>
      <div><small>Unidades disponibles</small><b>{available}</b></div>
      <div><small>Pedido actual</small><b>{selected}</b></div>
    </div>
    {notice&&<div role="status" className={notice.error?'notice error':'notice'}>{notice.text}</div>}
    <div className="layout">
      <section className="panel"><div className="heading"><h2>Inventario</h2>
        <button disabled={busy} onClick={()=>{setLoading(true);refresh().catch(e=>setNotice({error:true,text:message(e)})).finally(()=>setLoading(false))}}>Actualizar</button></div>
        {loading?<p role="status">Cargando productos…</p>:items.length===0?<p>No hay productos registrados.</p>:
          <ul className="catalog">{items.map(p=><li key={p.id}><div><strong>{p.name}</strong><small>{p.sku} · {p.available} disponibles</small></div>
            <button disabled={busy||(basket[p.id]??0)>=p.available} aria-label={`Agregar ${p.name}`} onClick={()=>dispatch({type:'add',product:p})}>Agregar +</button></li>)}</ul>}
      </section>
      <section className="panel"><h2>Nuevo producto</h2><form onSubmit={createProduct}>
        <label>SKU<input value={sku} onChange={e=>setSku(e.target.value)} maxLength={80} required/></label>
        <label>Nombre<input value={name} onChange={e=>setName(e.target.value)} maxLength={200} required/></label>
        <label>Existencias<input type="number" min="0" step="1" value={quantity} onChange={e=>setQuantity(Number(e.target.value))} required/></label>
        <button className="primary" disabled={busy}>Registrar producto</button>
      </form></section>
    </div>
    <section className="panel"><h2>Preparar pedido</h2>
      {lines.length?<ul className="catalog">{lines.map(line=><li key={line.productId}>
        <span>{items.find(p=>p.id===line.productId)?.name??line.productId} × {line.quantity}</span>
        <button disabled={busy} onClick={()=>dispatch({type:'remove',id:line.productId})}>Quitar</button></li>)}</ul>:
        <p>Selecciona productos del inventario.</p>}
      <button className="primary" disabled={busy||lines.length===0} onClick={createOrder}>Confirmar pedido</button></section>
    <footer>Proyecto demostrativo local, sin autenticación. No desplegar en Internet ni almacenar datos sensibles.</footer>
  </main>;
}

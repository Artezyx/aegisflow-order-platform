import { PublicClientApplication } from '@azure/msal-browser';
import { factories, models, service } from 'powerbi-client';
type Metrics={orders:number;revenue:number;marginPercent:number;openPipeline:number;currency:string};
const status=document.querySelector<HTMLElement>('#status')!;
const display=(id:string,s:string)=>{document.getElementById(id)!.textContent=s;};
const fmt=(n:number,c:string)=>new Intl.NumberFormat('en-US',{style:'currency',currency:c}).format(n);
async function metrics(){
 try{
  const response=await fetch((import.meta.env.VITE_API_BASE_URL||'http://localhost:5098')+'/api/metrics');
  if(!response.ok) throw new Error('HTTP '+response.status);
  const v=await response.json() as Metrics;
  display('revenue',fmt(v.revenue,v.currency));display('margin',v.marginPercent.toFixed(2)+'%');
  display('pipeline',fmt(v.openPipeline,v.currency));display('orders',String(v.orders));
  status.textContent='Live SQL Server metrics loaded.';
 }catch(e){status.textContent='SQL/API unavailable: '+String(e);}
}
void metrics();
document.getElementById('connect')!.addEventListener('click',async()=>{
 const clientId=import.meta.env.VITE_ENTRA_CLIENT_ID;
 const tenantId=import.meta.env.VITE_ENTRA_TENANT_ID;
 const reportId=import.meta.env.VITE_POWERBI_REPORT_ID;
 const embedUrl=import.meta.env.VITE_POWERBI_EMBED_URL;
 if(!clientId||!tenantId||!reportId||!embedUrl){status.textContent='Configure Entra and published Power BI report in web/.env.local.';return;}
 try{
  const msal=new PublicClientApplication({auth:{clientId,authority:'https://login.microsoftonline.com/'+tenantId,redirectUri:window.location.origin}});
  await msal.initialize();
  const scopes=['https://analysis.windows.net/powerbi/api/Report.Read.All'];
  const account=(await msal.loginPopup({scopes})).account;
  if(!account)throw Error('No signed-in account');
  let token:string;
  try{token=(await msal.acquireTokenSilent({account,scopes})).accessToken;}
  catch{token=(await msal.acquireTokenPopup({account,scopes})).accessToken;}
  const powerbi=new service.Service(factories.hpmFactory,factories.wpmpFactory,factories.routerFactory);
  powerbi.embed(document.getElementById('report')!,{type:'report',id:reportId,embedUrl,accessToken:token,tokenType:models.TokenType.Aad,permissions:models.Permissions.Read});
  status.textContent='Embedding requested; Power BI checks your report permissions.';
 }catch(e){status.textContent='Power BI sign-in/embed failed: '+String(e);}
});

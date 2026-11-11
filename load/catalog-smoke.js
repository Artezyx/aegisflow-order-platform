import http from 'k6/http';
import { check, sleep } from 'k6';
export const options={
  scenarios:{ catalog:{ executor:'constant-vus',vus:12,duration:'40s' } },
  thresholds:{http_req_failed:['rate<0.01'],http_req_duration:['p(95)<800']}
};
export default function(){
  if(!__ENV.JWT) throw new Error('JWT must be set to a short-lived local test token');
  const url=__ENV.BASE_URL||'http://localhost:5081';
  const response=http.get(url+'/api/products',{
    headers:{Authorization:'Bearer '+__ENV.JWT}
  });
  check(response,{'authorized catalog':r=>r.status===200});
  sleep(0.25);
}

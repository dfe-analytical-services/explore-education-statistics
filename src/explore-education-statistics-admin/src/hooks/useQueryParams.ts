import qs, { ParsedUrlQuery } from 'querystring';
import { useMemo } from 'react';
import { useLocation } from 'react-router';

export default function useQueryParams<Params extends ParsedUrlQuery>() {
  const location = useLocation();

  return useMemo<Params>(() => {
    return qs.parse(location.search.substr(1)) as Params;
  }, [location.search]);
}

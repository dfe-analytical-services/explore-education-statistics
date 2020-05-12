import { getConfig } from '@admin/config';
import useAsyncCallback from '@common/hooks/useAsyncCallback';
import useMounted from '@common/hooks/useMounted';

export default function useConfig() {
  const [state, run] = useAsyncCallback(getConfig);

  useMounted(() => {
    run();
  });

  return state;
}

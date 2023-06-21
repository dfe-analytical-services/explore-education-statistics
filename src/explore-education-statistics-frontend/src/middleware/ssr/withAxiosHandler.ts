import { Dictionary } from '@common/types';
import { isAxiosError } from 'axios';
import { GetServerSideProps } from 'next';

export default function withAxiosHandler<Props extends Dictionary<unknown>>(
  getServerSideProps: GetServerSideProps<Props>,
): GetServerSideProps<Props> {
  return async ctx => {
    try {
      return await getServerSideProps(ctx);
    } catch (e) {
      if (isAxiosError(e) && e.response?.status === 404) {
        return {
          notFound: true,
        };
      }

      throw e;
    }
  };
}

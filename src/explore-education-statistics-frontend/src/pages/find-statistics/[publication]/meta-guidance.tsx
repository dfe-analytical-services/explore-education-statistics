import { GetServerSideProps } from 'next';

export const getServerSideProps: GetServerSideProps = async ({ query }) => {
  return {
    redirect: {
      destination: `/find-statistics/${query.publication}/data-guidance`,
      permanent: false,
    },
  };
};

export default () => {};

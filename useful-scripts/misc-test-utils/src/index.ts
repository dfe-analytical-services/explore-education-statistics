import 'dotenv-safe/config';
import getAnswers from './utils/getAnswers';

process.env.NODE_TLS_REJECT_UNAUTHORIZED = '0';

const start = async () => {
  const choices = [
    'create new release & publication',
    'delete theme & topic',
    'create new publication',
    'create new release',
    'publish a new release',
    'upload subject',
    'create new methodology',
    'generate lorem ipsum content text block (methodology & release)',
    'publish all releases',
    'upload many subjects & publish',
  ] as const;

  getAnswers(choices);
};
start();

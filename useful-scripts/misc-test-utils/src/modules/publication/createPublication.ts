/* eslint-disable no-console */
import chalk from 'chalk';
import publicationService from '../../services/publicationService';

const { ADMIN_URL } = process.env;

const createPublication = async () => {
  const publicationId = await publicationService.createPublication();
  console.log(
    chalk.green(
      `Publication created: ${ADMIN_URL}/publication/${publicationId}/edit`,
    ),
  );
  return publicationId;
};
export default createPublication;

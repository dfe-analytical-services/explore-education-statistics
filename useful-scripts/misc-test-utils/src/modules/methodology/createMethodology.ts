import chalk from 'chalk';
import methodologyService from '../../services/methodologyService';

const createMethodology = async (publicationId: string) => {
  const methodologyId = await methodologyService.createMethodology(
    publicationId,
  );
  if (!methodologyId) {
    throw new Error(
      chalk.red(
        'No publicationId was returned from the "methodologyService.createMethodology" function!',
      ),
    );
  }

  return methodologyId;
};
export default createMethodology;

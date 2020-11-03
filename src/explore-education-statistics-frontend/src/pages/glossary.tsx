import {
  GlossaryEntry,
  GlossaryPageProps,
} from '@frontend/modules/glossary/GlossaryPage';
import fs from 'fs';
import { GetStaticProps } from 'next';
import path from 'path';

export { default } from '@frontend/modules/glossary/GlossaryPage';

// N.B we have placed `getStaticProps` in here as we can't access
// `fs` or `path` modules from non-page directories.
export const getStaticProps: GetStaticProps<GlossaryPageProps> = async () => {
  const htmlDir = path.join(process.cwd(), 'src/html/glossary');
  const filenames = fs.readdirSync(htmlDir);

  const entries: GlossaryEntry[] = filenames.map(filename => {
    const heading = path.parse(filename).name;

    const filePath = path.join(htmlDir, filename);
    const content = fs.readFileSync(filePath, 'utf8');

    return {
      heading,
      content,
    };
  });

  return {
    props: {
      entries,
    },
  };
};

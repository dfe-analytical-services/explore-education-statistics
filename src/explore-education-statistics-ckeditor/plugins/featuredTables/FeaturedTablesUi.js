import { ButtonView, Plugin } from 'ckeditor5';

export default class FeaturedTablesUI extends Plugin {
  init() {
    const { editor } = this;
    const config = editor.config.get('featuredTables') || {};

    editor.ui.componentFactory.add('featuredTables', locale => {
      const command = editor.commands.get('addFeaturedTableLink');
      const buttonView = new ButtonView(locale);

      buttonView.set({
        label: editor.t('Insert featured table link'),
        withText: true,
      });

      // Bind the state of the button to the command.
      buttonView.bind('isOn', 'isEnabled').to(command, 'value', 'isEnabled');

      // Execute the command when the button is clicked (executed).
      this.listenTo(buttonView, 'execute', () => {
        config.addFeaturedTableLink();
      });

      return buttonView;
    });
  }
}

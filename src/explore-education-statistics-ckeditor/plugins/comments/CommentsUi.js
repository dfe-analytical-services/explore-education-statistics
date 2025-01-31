import ButtonView from '@ckeditor/ckeditor5-ui/src/button/buttonview';
import Plugin from '@ckeditor/ckeditor5-core/src/plugin';
import CommentIcon from './comment-icon.svg';

export default class CommentsUI extends Plugin {
  init() {
    const { editor } = this;
    const config = editor.config.get('comments') || {};

    editor.ui.componentFactory.add('comment', locale => {
      const command = editor.commands.get('addCommentPlaceholder');
      const buttonView = new ButtonView(locale);

      buttonView.set({
        label: editor.t('Add comment'),
        tooltip: true,
        icon: CommentIcon,
      });

      // Bind the state of the button to the command.
      buttonView.bind('isOn', 'isEnabled').to(command, 'value', 'isEnabled');

      // Execute the command when the button is clicked (executed).
      this.listenTo(buttonView, 'execute', () => {
        config.addComment();
        editor.execute('addCommentPlaceholder');
      });

      return buttonView;
    });
  }
}

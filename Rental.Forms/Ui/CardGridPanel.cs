namespace Rental.Forms.Ui;

internal class CardGridPanel : Panel
{
    private readonly FlowLayoutPanel _flow = new()
    {
        Dock = DockStyle.Fill,
        AutoScroll = true,
        WrapContents = true,
        Padding = new Padding(0, 8, 0, 0)
    };

    public CardGridPanel()
    {
        Dock = DockStyle.Fill;
        BackColor = Color.Transparent;
        Controls.Add(_flow);
    }

    public void ClearCards() => _flow.Controls.Clear();

    public void AddCard(EntityCardControl card) => _flow.Controls.Add(card);

    public int CardCount => _flow.Controls.Count;
}

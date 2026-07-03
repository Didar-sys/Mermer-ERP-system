using Mermer.Data.Models;

#nullable disable
namespace Mermer.StockManagement.Models;

public class StockNameComposerValue : BindableObject
{
    private string _id; // Додаємо змінну для Id
    private int _order;
    private string _name;
    private string _shortName;

    // Додаємо саму властивість Id, яку вимагає валідатор
    public virtual string Id
    {
        get => this._id;
        set => this.SetProperty<string>(ref this._id, value, nameof(Id));
    }

    public virtual int Order
    {
        get => this._order;
        set => this.SetProperty<int>(ref this._order, value, nameof(Order));
    }

    public virtual string Name
    {
        get => this._name;
        set => this.SetProperty<string>(ref this._name, value, nameof(Name));
    }

    public virtual string ShortName
    {
        get => this._shortName;
        set => this.SetProperty<string>(ref this._shortName, value, nameof(ShortName));
    }

    public string Fullname => $"{this.Name} ({this.ShortName})";
}
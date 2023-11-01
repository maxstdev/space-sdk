using Newtonsoft.Json;
using System.Collections.Generic;

namespace MaxstXR.Place
{
    public enum CategoryDepth : int
    {
        First = 0,
        Second = 1,
        Third = 2,
    }

    public class CategoryName
    {
        [JsonProperty("ko")] public string ko;
        [JsonProperty("en")] public string en;
    }

    public class ThirdCategory : BaseCategory
    {
        [JsonProperty("category_icon")] public string categoryIcon;
        [JsonProperty("category_level")] public int categoryLevel;

        public override CategoryDepth GetCategoryDepth()
        {
            return CategoryDepth.Third;
        }
    }

    public class SecondCategory : BaseCategory
    {
        [JsonProperty("third_category_list")] public List<ThirdCategory> thirdcategoryList;
        [JsonProperty("category_icon")] public string categoryIcon;
        [JsonProperty("category_level")] public int categoryLevel;

        public override CategoryDepth GetCategoryDepth()
        {
            return CategoryDepth.Second;
        }
    }

    public class FirstCategory : BaseCategory
    {
        [JsonProperty("second_category_list")] public List<SecondCategory> secondCategoryList;

        public override CategoryDepth GetCategoryDepth()
        {
            return CategoryDepth.First;
        }
    }

    public abstract class BaseCategory : IEqualityComparer<BaseCategory>
    {
        [JsonProperty("category_id")] public int categoryId;
        [JsonProperty("category_name")] public CategoryName categoryName;
        [JsonProperty("description")] public string description;
        [JsonProperty("is_joint_type")] public bool? isJointType;
        [JsonProperty("poi_category_joint_type_response")] public JointType jointType;
        [JsonProperty("poi_category_augment_type_response")] public AugmentType augmentType;

        [JsonIgnore] public bool IsShow { get; set; } = true;
        [JsonIgnore] public BaseCategory Parent { get; set; } = null;

        public bool Equals(BaseCategory x, BaseCategory y)
        {
            return x.categoryId == y.categoryId;
        }

        public int GetHashCode(BaseCategory obj)
        {
            return obj.categoryId.GetHashCode();
        }

        public BaseCategory GetFirstParent()
        {
            if(GetCategoryDepth() != CategoryDepth.First)
            {
                return Parent.GetFirstParent();
            }
            else
            {
                return this;
            }
        }

        public abstract CategoryDepth GetCategoryDepth();
    }
}

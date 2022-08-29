﻿using MapControl;
using PhotoLocator.Metadata;

namespace PhotoLocator
{
    [TestClass]
    public class AutoTagViewModelTest
    {
        [TestMethod]
        public async Task AutoTag()
        {
            var file = new PictureItemViewModel(@"TestData\2022-06-17_19.03.02.jpg", false);
            file.IsSelected = true;
            await file.LoadMetadataAndThumbnailAsync(CancellationToken.None);

            var trace = new GpsTrace();
            var location = new Location(1, 2);
            trace.Locations.Add(location);
            trace.TimeStamps.Add(file.TimeStamp!.Value.AddMinutes(1));

            var vm = new AutoTagViewModel(new[] { file }, new[] { file }, new[] { trace }, () => { });
            vm.MaxTimestampDifference = 2;
            vm.AutoTag(vm.GpsTraces);

            Assert.AreEqual(location.Latitude, file.GeoTag!.Latitude);
            Assert.AreEqual(location.Longitude, file.GeoTag.Longitude);
        }
    }
}